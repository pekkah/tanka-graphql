using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets.Results;

public partial class ConnectionAckResult(
    GraphQLTransportWSProtocol protocol,
    ILogger<ConnectionAckResult> logger) : IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        if (context.Message.Type != MessageTypes.ConnectionInit)
        {
            Log.ExpectedInitMessageGot(logger, context.Message.Type);
            await context.Close(new WebSocketCloseStatusException(
                CloseCode.Unauthorized,
                $"Expected {MessageTypes.ConnectionInit}"));

            return;
        }

        protocol.ConnectionInitReceived = true;
        Log.ConnectionAck(logger);
        await context.Write(new ConnectionAck());
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Expected 'connection_init' got '{type}'")]
        public static partial void ExpectedInitMessageGot(ILogger logger, string type);

        [LoggerMessage(LogLevel.Information, "Connection ack")]
        public static partial void ConnectionAck(ILogger logger);
    }
}