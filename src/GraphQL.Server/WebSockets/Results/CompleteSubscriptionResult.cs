using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets.Results;

public partial class CompleteSubscriptionResult(
    SubscriptionManager subscriptions,
    ILogger<CompleteSubscriptionResult> logger): IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        if (context.Message is not Complete complete)
        {
            Log.InvalidMessageType(logger, MessageTypes.Complete, context.Message.Type);
            await context.Close(new WebSocketCloseStatusException(
                CloseCode.BadRequest,
                $"Expected {MessageTypes.Complete}"));
            
            return;
        }

        await subscriptions.Dequeue(complete.Id);
    }

    public static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Expected '{Expected}' but got '{Actual}'.")]
        public static partial void InvalidMessageType(ILogger logger, string expected, string actual);
    }
}