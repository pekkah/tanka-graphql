using System.Buffers;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;

namespace Tanka.GraphQL.Server.WebSockets;

public class WebSocketChannel(WebSocket webSocket, JsonSerializerOptions jsonOptions)
{
    private readonly Channel<MessageBase> _input = Channel.CreateUnbounded<MessageBase>();
    private readonly Channel<MessageBase> _output = Channel.CreateUnbounded<MessageBase>();

    public ChannelReader<MessageBase> Reader => _input.Reader;

    public ChannelWriter<MessageBase> Writer => _output.Writer;

    public async Task Run()
    {
        Task receiving = StartReceiving(webSocket, _input.Writer, jsonOptions);
        Task writing = StartWriting(webSocket, _output.Reader, jsonOptions);

        await Task.WhenAll(receiving, writing);
    }

    private static async Task StartWriting(
        WebSocket webSocket,
        ChannelReader<MessageBase> reader,
        JsonSerializerOptions jsonSerializerOptions)
    {
        while (await reader.WaitToReadAsync() && webSocket.State == WebSocketState.Open)
            if (reader.TryRead(out MessageBase? data))
            {
                byte[] buffer =
                    JsonSerializer.SerializeToUtf8Bytes(data, jsonSerializerOptions);

                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }

        await reader.Completion;
    }


    private static async Task StartReceiving(
        WebSocket webSocket,
        ChannelWriter<MessageBase> writer,
        JsonSerializerOptions jsonSerializerOptions)
    {
        Exception? error = null;
        var buffer = new ArrayBufferWriter<byte>(1024);
        while (webSocket.State == WebSocketState.Open)
        {
            Memory<byte> readBuffer = buffer.GetMemory(1024);
            ValueWebSocketReceiveResult result = await webSocket.ReceiveAsync(readBuffer, CancellationToken.None);
            buffer.Advance(result.Count);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                break;
            }

            if (result.EndOfMessage)
            {
                var message = JsonSerializer.Deserialize<MessageBase>(
                    buffer.WrittenSpan,
                    jsonSerializerOptions
                );

                if (message is not null)
                    try
                    {
                        await writer.WriteAsync(message);
                    }
                    catch (ChannelClosedException)
                    {
                        break;
                    }

                buffer.ResetWrittenCount();
            }
        }

        writer.TryComplete(error);
    }

    public void Complete(Exception? error = null)
    {
        Writer.TryComplete(error);
    }
}