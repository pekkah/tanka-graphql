using System.Net.WebSockets;

using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets.Results;

public partial class WebSocketCloseResult(
    WebSocketCloseStatus closeCode,
    ILogger<WebSocketCloseResult> logger) : IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        Log.WebSocketClosed(logger, closeCode);
        await context.Close(new WebSocketCloseStatusException(closeCode));
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "WebSocket closed because of '{CloseCode}'")]
        public static partial void WebSocketClosed(
            ILogger logger, 
            WebSocketCloseStatus closeCode);
    }
}