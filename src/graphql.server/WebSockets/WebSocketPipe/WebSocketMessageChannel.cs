using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data.Common;
using System.IO.Pipelines;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Tanka.GraphQL.Server.WebSockets.WebSocketPipe
{
    public partial class WebSocketMessageChannel<T>
    {
        private readonly WebSocket _socket;
        private readonly ILogger<WebSocketMessageChannel<T>> _logger;
        private Channel<T> Input { get; }

        private Channel<T> Output { get; }

        public ChannelReader<T> Reader => Input.Reader;

        public ChannelWriter<T> Writer => Output.Writer;

        protected Pipe _application { get; }

        private readonly TimeSpan _closeTimeout = TimeSpan.FromSeconds(5);
        private bool _aborted;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public WebSocketMessageChannel(WebSocket socket, ILogger<WebSocketMessageChannel<T>> logger)
        {
            _socket = socket;
            _logger = logger;
            _application = new Pipe();
            Input = Channel.CreateUnbounded<T>();
            Output = Channel.CreateUnbounded<T>();
        }

        public async Task ProcessSocketAsync(CancellationToken cancellationToken)
        {
            // Begin sending and receiving. Receiving must be started first because ExecuteAsync enables SendAsync.
            var receiving = StartReadSocket(_socket, cancellationToken);
            var processing = StartReadMessages(cancellationToken);
            var sending = StartSending(_socket);

            // Wait for send or receive to complete
            var trigger = await Task.WhenAny(receiving, sending, processing);

            if (trigger == receiving)
            {
                Log.WaitingForSend(_logger);

                // We're waiting for the application to finish and there are 2 things it could be doing
                // 1. Waiting for application data
                // 2. Waiting for a websocket send to complete

                // Cancel the application so that ReadAsync yields
                _application.Reader.CancelPendingRead();

                using (var delayCts = new CancellationTokenSource())
                {
                    var resultTask = await Task.WhenAny(sending, Task.Delay(_closeTimeout, delayCts.Token));

                    if (resultTask != sending)
                    {
                        // We timed out so now we're in ungraceful shutdown mode
                        Log.CloseTimedOut(_logger);

                        // Abort the websocket if we're stuck in a pending send to the client
                        _aborted = true;

                        _socket.Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
            }
            else
            {
                Log.WaitingForClose(_logger);

                // We're waiting on the websocket to close and there are 2 things it could be doing
                // 1. Waiting for websocket data
                // 2. Waiting on a flush to complete (backpressure being applied)

                using (var delayCts = new CancellationTokenSource())
                {
                    var resultTask = await Task.WhenAny(receiving, Task.Delay(_closeTimeout, delayCts.Token));

                    if (resultTask != receiving)
                    {
                        // Abort the websocket if we're stuck in a pending receive from the client
                        _aborted = true;

                        _socket.Abort();

                        // Cancel any pending flush so that we can quit
                        _application.Writer.CancelPendingFlush();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
            }
        }

        private async Task StartReadMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var messageResult = await _application.Reader.ReadAsync(cancellationToken);

                if (messageResult.IsCanceled)
                    break;

                var buffer = messageResult.Buffer;
                var message = ReadMessageCore(buffer);

                await Input.Writer.WriteAsync(message);
                _application.Reader.AdvanceTo(buffer.End);

                if (messageResult.IsCompleted)
                    break;
            }

            T? ReadMessageCore(ReadOnlySequence<byte> buffer)
            {
                var reader = new Utf8JsonReader(buffer);
                return JsonSerializer.Deserialize<T>(ref reader, _jsonOptions);
            }
        }

        private async Task StartReadSocket(WebSocket socket, CancellationToken cancellationToken)
        {
            var token = cancellationToken;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                    var result = await socket.ReceiveAsync(Memory<byte>.Empty, token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    var memory = _application.Writer.GetMemory();

                    var receiveResult = await socket.ReceiveAsync(memory, token);

                    // Need to check again for netcoreapp3.0 and later because a close can happen between a 0-byte read and the actual read
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    Log.MessageReceived(
                        _logger, 
                        receiveResult.MessageType, 
                        receiveResult.Count,
                        receiveResult.EndOfMessage);

                    _application.Writer.Advance(receiveResult.Count);

                    if (receiveResult.EndOfMessage)
                    {
                        var flushResult = await _application.Writer.FlushAsync();
                    
                        // We canceled in the middle of applying back pressure
                        // or if the consumer is done
                        if (flushResult.IsCanceled || flushResult.IsCompleted)
                        {
                            break;
                        }
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // Client has closed the WebSocket connection without completing the close handshake
                Log.ClosedPrematurely(_logger, ex);
            }
            catch (OperationCanceledException)
            {
                // Ignore aborts, don't treat them like transport errors
            }
            catch (Exception ex)
            {
                if (!_aborted && !token.IsCancellationRequested)
                {
                    _application.Writer.Complete(ex);
                }
            }
            finally
            {
                // We're done writing
                _application.Writer.Complete();
            }
        }

        private async Task StartSending(WebSocket socket)
        {
            Exception? error = null;

            try
            {
                while (true)
                {
                    var message = await Output.Reader.ReadAsync();
                    var bytes = JsonSerializer.SerializeToUtf8Bytes<T>(message, _jsonOptions);

                    //todo: do we need cancellation token
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }

            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                // Send the close frame before calling into user code
                if (WebSocketCanSend(socket))
                {
                    try
                    {
                        // We're done sending, send the close frame to the client if the websocket is still open
                        await socket.CloseOutputAsync(
                            error != null
                                ? WebSocketCloseStatus.InternalServerError
                                : WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Log.ClosingWebSocketFailed(_logger, ex);
                    }
                }
            }
        }

        private static bool WebSocketCanSend(WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                   ws.State == WebSocketState.Closed ||
                   ws.State == WebSocketState.CloseSent);
        }

        public async Task Complete(WebSocketCloseStatus? webSocketCloseStatus, string? description = null)
        {
            await _socket.CloseOutputAsync(webSocketCloseStatus ?? WebSocketCloseStatus.NormalClosure, description, CancellationToken.None);
        }
    }
}
