using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets;

public class WebSocketChannel(DuplexWebSocketPipe webSocketPipe, ILogger<WebSocketChannel> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Channel<MessageBase> _fromTransport =
        Channel.CreateUnbounded<MessageBase>();
    
    public ChannelReader<MessageBase> Reader => _fromTransport.Reader;
    
    public async Task Run()
    {
        webSocketPipe.Start();
        
        Task read = StartReading();
        
        await Task.WhenAll(read);

        await webSocketPipe.Complete();
    }

    public async Task Write(MessageBase message)
    {
        var buffer = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        await webSocketPipe.Write(buffer);
    }


    private async Task StartReading()
    {
        while (true)
        {
            ReadResult result = await webSocketPipe.Reader.ReadAsync(
                CancellationToken.None
            );

            if (result.IsCanceled)
                break;

            if (result.IsCompleted)
                break;

            ReadOnlySequence<byte> buffer = result.Buffer;
            MessageBase? message = Deserialize(ref buffer);
            if (message != null)
            {
                logger.LogDebug("Received message {MessageType}", message.Type);
                await _fromTransport.Writer.WriteAsync(message, CancellationToken.None);
                webSocketPipe.Reader.AdvanceTo(buffer.End);
            }
            else
                break;

        }
    }

    private MessageBase? Deserialize(ref ReadOnlySequence<byte> messageBuffer)
    {
        try
        {
            var reader = new Utf8JsonReader(messageBuffer);
            var message = JsonSerializer.Deserialize<MessageBase>(ref reader, JsonOptions);

            return message;
        }
        catch (Exception x)
        {
            return null;
        }
    }

    public static WebSocketChannel Create(WebSocket webSocket, ILoggerFactory loggerFactory)
    {
        var webSocketPipe = new DuplexWebSocketPipe(webSocket);
        return new WebSocketChannel(webSocketPipe, loggerFactory.CreateLogger<WebSocketChannel>());
    }

    private volatile bool _isCompleted;
    
    public async Task Complete(Exception? error = null)
    {
        if (_isCompleted)
            return;
        
        _isCompleted = true;
        _fromTransport.Writer.Complete(error);

        await webSocketPipe.Complete(error);
    }
}