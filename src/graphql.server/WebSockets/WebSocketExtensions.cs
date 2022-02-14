using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Server.WebSockets;

internal static class WebSocketExtensions
{
    public static ValueTask SendAsync(this WebSocket webSocket, ReadOnlySequence<byte> buffer,
        WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
    {
        if (buffer.IsSingleSegment)
            return webSocket.SendAsync(buffer.First, webSocketMessageType, true, cancellationToken);
        return SendMultiSegmentAsync(webSocket, buffer, webSocketMessageType, cancellationToken);
    }

    private static async ValueTask SendMultiSegmentAsync(WebSocket webSocket, ReadOnlySequence<byte> buffer,
        WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
    {
        var position = buffer.Start;
        while (buffer.TryGet(ref position, out var segment))
            await webSocket.SendAsync(segment, webSocketMessageType, false, cancellationToken);

        // Empty end of message frame
        await webSocket.SendAsync(Memory<byte>.Empty, webSocketMessageType, true, cancellationToken);
    }
}