using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using fugu.graphql.type;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerClients
    {
        private readonly ConcurrentDictionary<string, QueryManager> _clients =
            new ConcurrentDictionary<string, QueryManager>();

        private readonly ISchema _schema;

        public ServerClients(ISchema schema)
        {
            _schema = schema;
        }

        public Task OnConnectedAsync(HubCallerContext context)
        {
            _clients[context.ConnectionId] = new QueryManager(_schema);
            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(HubCallerContext context, Exception exception)
        {
            if (_clients.TryGetValue(context.ConnectionId, out var queryManager)) return queryManager.CloseAllAsync();

            return Task.CompletedTask;
        }

        public Task<QueryStream> QueryAsync(HubCallerContext context, QueryRequest query,
            CancellationToken cancellationToken)
        {
            if (!_clients.TryGetValue(context.ConnectionId, out var queryManager))
                throw new InvalidOperationException($"No QueryManager for connection '{context.ConnectionId}'");

            return queryManager.QueryAsync(query, cancellationToken);
        }
    }
}