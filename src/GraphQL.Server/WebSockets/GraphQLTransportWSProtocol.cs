using Microsoft.Extensions.Logging;

using Tanka.GraphQL.Server.WebSockets.Results;

namespace Tanka.GraphQL.Server.WebSockets;

public class GraphQLTransportWSProtocol(
    SubscriptionManager subscriptions,
    ILoggerFactory loggerFactory)
{
    public bool ConnectionInitReceived = false;

    public IMessageResult Accept(MessageBase message)
    {
        if (!ConnectionInitReceived)
            return new ConnectionAckResult(
                this,
                loggerFactory.CreateLogger<ConnectionAckResult>()
            );

        return message.Type switch
        {
            MessageTypes.ConnectionInit => new WebSocketCloseResult(
                CloseCode.TooManyInitialisationRequests,
                loggerFactory.CreateLogger<WebSocketCloseResult>()),
            MessageTypes.Ping => new PongResult(loggerFactory.CreateLogger<PongResult>()),
            MessageTypes.Subscribe => new SubscribeResult(
                subscriptions, 
                loggerFactory.CreateLogger<SubscribeResult>()),
            MessageTypes.Complete => new Results.CompleteSubscriptionResult(
                subscriptions,
                loggerFactory.CreateLogger<Results.CompleteSubscriptionResult>()),
            _ => new UnknownMessageResult(loggerFactory.CreateLogger<UnknownMessageResult>())
        };
    }
}