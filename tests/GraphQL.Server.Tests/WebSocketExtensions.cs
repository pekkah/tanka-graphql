using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

namespace Tanka.GraphQL.Server.Tests;

internal static class WebSocketExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task Send<T>(this WebSocket webSocket, T message)
    {
        var buffer = JsonSerializer.SerializeToUtf8Bytes(
            message, 
            JsonOptions);
        
        await webSocket.SendAsync(
            buffer, 
            WebSocketMessageType.Text, 
            true, 
            CancellationToken.None);
    }

    public static async Task<MessageBase> Receive(
        this WebSocket webSocket,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        return await webSocket.Receive(cts.Token);
    }

    public static async Task<MessageBase> Receive(
        this WebSocket webSocket, 
        CancellationToken cancellationToken = default)
    {
        var buffer = new ArraySegment<byte>(new byte[1024*8]);
        using var memoryStream = new MemoryStream();
        
        do
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);

            if (result.CloseStatus != null)
                throw new InvalidOperationException($"{result.CloseStatus}:{result.CloseStatusDescription}");
            
            memoryStream.Write(buffer.Slice(0, result.Count));

            if (result.EndOfMessage)
            {
                return JsonSerializer.Deserialize<MessageBase>(
                    memoryStream.ToArray(), 
                    JsonOptions);
                
            }
        } while (true);
    }
}