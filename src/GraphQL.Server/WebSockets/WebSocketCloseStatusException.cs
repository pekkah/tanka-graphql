using System.Net.WebSockets;

namespace Tanka.GraphQL.Server.WebSockets;

internal class WebSocketCloseStatusException(
    WebSocketCloseStatus closeStatus, 
    string? message = default,
    Exception? inner = default)
    : Exception(message, inner)
{
    public WebSocketCloseStatus WebSocketCloseStatus => closeStatus;
}