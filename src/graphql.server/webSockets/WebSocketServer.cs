using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using tanka.graphql.execution;

namespace tanka.graphql.server.webSockets
{
    public class WebSocketServer
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProtocolHandler _protocol;
        private ILogger<WebSocketServer> _logger;

        public WebSocketServer(
            ILoggerFactory loggerFactory,
            IProtocolHandler protocol)
        {
            _loggerFactory = loggerFactory;
            _protocol = protocol;
            _logger = loggerFactory.CreateLogger<WebSocketServer>();
        }

        public ConcurrentDictionary<ConnectionInfo, MessageServer> Clients { get; } =
            new ConcurrentDictionary<ConnectionInfo, MessageServer>();

        public async Task ProcessRequestAsync(HttpContext context)
        {
            MessageServer messageServer = null;
            try
            {
                var connection = new WebSocketConnection(_loggerFactory);
                messageServer = new SubscriptionServer(_protocol);
                
                Clients.TryAdd(context.Connection, messageServer);
                var run = messageServer.RunAsync(connection, context.RequestAborted);
                await connection.ProcessRequestAsync(context);
                messageServer.Complete();
                await run;
            }
            catch (Exception e)
            {
                messageServer?.Complete(e);
                throw;
            }
            finally
            {
                if (Clients.TryRemove(context.Connection, out var s))
                {
                    s.Complete();
                    await s.Completion;
                }
            }
        }
    }

    public interface ISubscriptionServerFactory
    {
    }
}