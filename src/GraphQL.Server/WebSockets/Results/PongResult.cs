using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server.WebSockets.Results;

public partial class PongResult(ILogger<PongResult> logger) : IMessageResult
{
    public async Task Execute(IMessageContext context)
    {
        Log.Pong(logger);
        await context.Write(new Pong());
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Debug, "Ping <-> Pong")]
        public static partial void Pong(ILogger logger);
    }
}