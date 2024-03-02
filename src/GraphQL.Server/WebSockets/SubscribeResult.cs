using Microsoft.Extensions.Logging;
using Tanka.GraphQL.Request;

namespace Tanka.GraphQL.Server.WebSockets;

public partial class SubscribeResult(
    SubscriptionManager subscriptions,
    ILogger<SubscribeResult> logger) : IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        if (context.Message is not Subscribe subscribe)
        {
            Log.ExpectedSubscribeMessageGot(logger, context.Message.Type);
            await context.Close(new WebSocketCloseStatusException(
                CloseCode.BadRequest,
                $"Expected {MessageTypes.Subscribe}"));

            return;
        }

        ArgumentException.ThrowIfNullOrEmpty(subscribe.Id);

        if (!subscriptions.Enqueue(subscribe.Id, subscribe.Payload))
        {
            await context.Close(new WebSocketCloseStatusException(
                               CloseCode.BadRequest, 
                               "Subscription id is not unique")
            );
        }
    }

    public static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Expected 'subscribe' got '{type}'")]
        public static partial void ExpectedSubscribeMessageGot(ILogger logger, string type);

        [LoggerMessage(5, LogLevel.Debug, "Subscription({Id}) - Result({elapsed}): {result}")]
        public static partial void ExecutionResult(ILogger logger, string id, ExecutionResult? result, string elapsed);

        [LoggerMessage(3, LogLevel.Debug, "Subscription({Id}) - Request: {request}")]
        public static partial void Request(ILogger logger, string id, GraphQLRequest request);

        [LoggerMessage(10, LogLevel.Information,
            "Subscription({Id}) - Server stream completed. {count} messages sent.")]
        public static partial void Completed(ILogger logger, string id, ulong count);

        [LoggerMessage(0, LogLevel.Error,
            "Subscription({Id}) - Subscription id  is not unique")]
        public static partial void SubscriberAlreadyExists(ILogger logger, string id);
    }
}