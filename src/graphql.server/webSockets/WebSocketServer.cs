using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace tanka.graphql.server.webSockets
{
    public class WebSocketServer
    {
        private readonly ILoggerFactory _loggerFactory;
        private ILogger<WebSocketServer> _logger;

        public WebSocketServer(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<WebSocketServer>();
        }

        public ConcurrentDictionary<ConnectionInfo, GraphQLServer> Clients { get; } =
            new ConcurrentDictionary<ConnectionInfo, GraphQLServer>();

        public async Task ProcessRequestAsync(HttpContext context)
        {
            GraphQLServer server = null;
            try
            {
                var connection = new WebSocketConnection(_loggerFactory);
                server = new GraphQLServer(connection);

                Clients.TryAdd(context.Connection, server);
                var run = server.RunAsync(context.RequestAborted);
                await connection.ProcessRequestAsync(context);
                server.Complete();
                await run;
            }
            catch (Exception e)
            {
                server?.Complete(e);
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
}