using System.IO.Pipelines;
using System.Net.WebSockets;

namespace Tanka.GraphQL.Server.WebSockets;

public class DuplexWebSocketPipe(WebSocket webSocket)
{
    private readonly Pipe _fromWebSocket = new();
    private bool _isCompleted;

    public PipeReader Reader => _fromWebSocket.Reader;

    public Task Running { get; private set; } = Task.CompletedTask;

    public void Start()
    {
        Running = ProcessSocketAsync();
    }

    private async Task ProcessSocketAsync()
    {
        await StartReceiving();
        _fromWebSocket.Reader.CancelPendingRead();
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        try
        {
            await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await _fromWebSocket.Writer.CompleteAsync(ex);
        }
    }

    private async Task StartReceiving()
    {
        try
        {
            while (true)
            {
                // Do a 0 byte read so that idle connections don't allocate a buffer when waiting for a read
                //ValueWebSocketReceiveResult result = await webSocket.ReceiveAsync(Memory<byte>.Empty, CancellationToken.None);

                //if (result.MessageType == WebSocketMessageType.Close) return;

                Memory<byte> memory = _fromWebSocket.Writer.GetMemory(512);
                var result = await webSocket.ReceiveAsync(memory, CancellationToken.None);
                _fromWebSocket.Writer.Advance(result.Count);

                // Need to check again for netcoreapp3.0 and later because a close can happen between a 0-byte read and the actual read
                if (result.MessageType == WebSocketMessageType.Close) return;

                if (result.EndOfMessage)
                {
                    FlushResult flushResult = await _fromWebSocket.Writer.FlushAsync();

                    // We canceled in the middle of applying back pressure
                    // or if the consumer is done
                    if (flushResult.IsCanceled || flushResult.IsCompleted) break;
                }
            }
        }
        catch (Exception ex)
        {
            await Complete(ex);
        }
        finally
        {
            await Complete();
        }
    }

    public async ValueTask Complete(Exception? error = null)
    {
        if (_isCompleted)
            return;

        _isCompleted = true;
        if (error != null && WebSocketCanSend())
        {
            if (error is WebSocketCloseStatusException closeStatus)
                await webSocket.CloseAsync(closeStatus.WebSocketCloseStatus, closeStatus.Message,
                    CancellationToken.None);
            else
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, error.Message,
                    CancellationToken.None);
        }

        await _fromWebSocket.Writer.CompleteAsync(error);
    }

    private bool WebSocketCanSend()
    {
        return !(webSocket.State == WebSocketState.Aborted ||
                 webSocket.State == WebSocketState.Closed ||
                 webSocket.State == WebSocketState.CloseSent);
    }
}