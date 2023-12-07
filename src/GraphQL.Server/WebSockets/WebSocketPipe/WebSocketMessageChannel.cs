using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

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

        private Pipe Application { get; }

        private readonly TimeSpan _closeTimeout = TimeSpan.FromSeconds(5);
        private bool _aborted;

        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public WebSocketMessageChannel(WebSocket socket, ILogger<WebSocketMessageChannel<T>> logger)
        {
            _socket = socket;
            _logger = logger;
            Application = new Pipe();
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
                Application.Reader.CancelPendingRead();

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
                        await delayCts.CancelAsync();
                    }
                }
            }
            else
            {
                Log.WaitingForClose(_logger);

                // We're waiting on the websocket to close and there are 2 things it could be doing
                // 1. Waiting for websocket data
                // 2. Waiting on a flush to complete (backpressure being applied)

                using var delayCts = new CancellationTokenSource();
                var resultTask = await Task.WhenAny(receiving, Task.Delay(_closeTimeout, delayCts.Token));

                if (resultTask != receiving)
                {
                    // Abort the websocket if we're stuck in a pending receive from the client
                    _aborted = true;

                    _socket.Abort();

                    // Cancel any pending flush so that we can quit
                    Application.Writer.CancelPendingFlush();
                }
                else
                {
                    await delayCts.CancelAsync();
                }
            }
        }

        private async Task StartReadMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var messageResult = await Application.Reader.ReadAsync(cancellationToken);

                if (messageResult.IsCanceled)
                    break;

                var buffer = messageResult.Buffer;
                var message = ReadMessageCore(buffer);

                if (message is null)
                    continue;
                
                await Input.Writer.WriteAsync(message);
                Application.Reader.AdvanceTo(buffer.End);

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

                    var memory = Application.Writer.GetMemory();

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

                    Application.Writer.Advance(receiveResult.Count);

                    if (receiveResult.EndOfMessage)
                    {
                        var flushResult = await Application.Writer.FlushAsync();
                    
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
                    await Application.Writer.CompleteAsync(ex);
                }
            }
            finally
            {
                // We're done writing
                await Application.Writer.CompleteAsync();
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
