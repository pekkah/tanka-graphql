using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets.Results;

public partial class UnknownMessageResult(
    ILogger<UnknownMessageResult> logger) : IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        Log.UnknownMessageType(logger, context.Message.Type);
        await context.Close(new WebSocketCloseStatusException(
            CloseCode.BadRequest,
            $"Message type '{context.Message.Type}' not supported"));
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Unknown message type of '{MessageType}' received")]
        public static partial void UnknownMessageType(ILogger logger, string messageType);
    }
}