using System.Net.WebSockets;
using System.Text.Json;

namespace Tanka.GraphQL.Server.WebSockets;

public static class EchoProtocol
{
    public const string Protocol = "echo-ws";

    public static async Task Run(WebSocket webSocket)
    {
        var channel = new WebSocketChannel(webSocket, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var echo = Echo(channel);

        await Task.WhenAll(channel.Run(), echo);
    }

    private static async Task Echo(WebSocketChannel channel)
    {
        while (await channel.Reader.WaitToReadAsync())
        {
            if (channel.Reader.TryRead(out var message))
                await channel.Writer.WriteAsync(message);
        }

        channel.Complete();
    }
}