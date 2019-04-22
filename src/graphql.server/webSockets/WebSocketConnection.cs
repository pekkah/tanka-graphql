using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace tanka.graphql.server.webSockets
{
    public partial class WebSocketConnection : IDuplexPipe
    {
        private readonly TimeSpan _closeTimeout;
        private readonly ILogger<WebSocketConnection> _logger;
        private volatile bool _aborted;
        private Pipe _writePipe;
        private Pipe _readPipe;

        public WebSocketConnection(ILoggerFactory loggerFactory)
        {
            _writePipe = new Pipe();
            _readPipe = new Pipe();
            _logger = loggerFactory.CreateLogger<WebSocketConnection>();
            _closeTimeout = TimeSpan.FromSeconds(2);
        }

        public async Task ProcessRequestAsync(HttpContext context)
        {
            var subProtocol = context.WebSockets.WebSocketRequestedProtocols.FirstOrDefault();
            using var ws = await context.WebSockets.AcceptWebSocketAsync(subProtocol);

            Log.SocketOpened(_logger, subProtocol);

            try
            {
                await ProcessSocketAsync(ws, context.RequestAborted);
            }
            finally
            {
                Log.SocketClosed(_logger);
            }
        }

        private async Task ProcessSocketAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            // Begin sending and receiving. Receiving must be started first because ExecuteAsync enables SendAsync.
            var receiving = StartReceiving(socket, cancellationToken);
            var sending = StartSending(socket);

            // Wait for send or receive to complete
            var trigger = await Task.WhenAny(receiving, sending);

            if (trigger == receiving)
            {
                Log.WaitingForSend(_logger);

                // We're waiting for the application to finish and there are 2 things it could be doing
                // 1. Waiting for application data
                // 2. Waiting for a websocket send to complete

                // Cancel the application so that ReadAsync yields
                _application.Input.CancelPendingRead();

                using var delayCts = new CancellationTokenSource();
                var resultTask = await Task.WhenAny(sending, Task.Delay(_closeTimeout, delayCts.Token));

                if (resultTask != sending)
                {
                    // We timed out so now we're in ungraceful shutdown mode
                    Log.CloseTimedOut(_logger);

                    // Abort the websocket if we're stuck in a pending send to the client
                    _aborted = true;

                    socket.Abort();
                }
                else
                {
                    delayCts.Cancel();
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

                    socket.Abort();

                    // Cancel any pending flush so that we can quit
                    _application.Output.CancelPendingFlush();
                }
                else
                {
                    delayCts.Cancel();
                }
            }
        }

        private async Task StartReceiving(WebSocket socket, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                    var emptyReadResult = await socket.ReceiveAsync(Memory<byte>.Empty, token);

                    if (emptyReadResult.MessageType == WebSocketMessageType.Close) return;

                    var memory = _application.Output.GetMemory();
                    var receiveResult = await socket.ReceiveAsync(memory, token);

                    // Need to check again for NetCoreApp2.2 because a close can happen between a 0-byte read and the actual read
                    if (receiveResult.MessageType == WebSocketMessageType.Close) return;

                    Log.MessageReceived(
                        _logger,
                        receiveResult.MessageType,
                        receiveResult.Count,
                        receiveResult.EndOfMessage);

                    _application.Output.Advance(receiveResult.Count);

                    if (receiveResult.EndOfMessage)
                    {
                        var flushResult = await _application.Output.FlushAsync();

                        // We canceled in the middle of applying back pressure
                        // or if the consumer is done
                        if (flushResult.IsCanceled || flushResult.IsCompleted) break;
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
                    _application.Output.Complete(ex);

                    // We re-throw here so we can communicate that there was an error when sending
                    // the close frame
                    throw;
                }
            }
            finally
            {
                // We're done writing
                _application.Output.Complete();
            }
        }

        private async Task StartSending(WebSocket socket)
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    var result = await _application.Input.ReadAsync();
                    var buffer = result.Buffer;

                    // Get a frame from the application

                    try
                    {
                        if (result.IsCanceled) break;

                        if (!buffer.IsEmpty)
                            try
                            {
                                Log.SendPayload(_logger, buffer.Length);

                                // apollo sub protocol uses json with text
                                var webSocketMessageType = WebSocketMessageType.Text;

                                if (WebSocketCanSend(socket))
                                    await socket.SendAsync(buffer, webSocketMessageType);
                                else
                                    break;
                            }
                            catch (Exception ex)
                            {
                                if (!_aborted) Log.ErrorWritingFrame(_logger, ex);
                                break;
                            }
                        else if (result.IsCompleted) break;
                    }
                    finally
                    {
                        _application.Input.AdvanceTo(buffer.End);
                    }
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
                    // We're done sending, send the close frame to the client if the websocket is still open
                    await socket.CloseOutputAsync(
                        error != null ? WebSocketCloseStatus.InternalServerError : WebSocketCloseStatus.NormalClosure,
                        "", CancellationToken.None);

                _application.Input.Complete();
            }
        }

        private static bool WebSocketCanSend(WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                     ws.State == WebSocketState.Closed ||
                     ws.State == WebSocketState.CloseSent);
        }

        private ValueTask ProcessLine(ReadOnlySequence<byte> slice)
        {
            var message = GetUtf8String(slice);
            return default;
        }

        private string GetUtf8String(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment) return Encoding.UTF8.GetString(buffer.First.Span);

            return string.Create((int) buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.UTF8.GetChars(segment.Span, span);

                    span = span.Slice(segment.Length);
                }
            });
        }

        public PipeReader Input => _readPipe.Reader;

        public PipeWriter Output => _writePipe.Writer;
    }
}