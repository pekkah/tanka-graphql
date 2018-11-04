using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerHub : Hub
    {
        private readonly ConcurrentDictionary<string, QueryManager> _clients = new ConcurrentDictionary<string, QueryManager>();

        public ServerHub()
        {
        }

        public override Task OnConnectedAsync()
        {
            _clients[Context.ConnectionId] = new QueryManager();
            return Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_clients.TryGetValue(Context.ConnectionId, out var queryManager))
            {
                await queryManager.CloseAllAsync();
            }
        }

        [HubMethodName("query")]
        public async Task<ChannelReader<ExecutionResult>> QueryAsync(QueryRequest query, CancellationToken cancellationToken)
        {
            if (!_clients.TryGetValue(Context.ConnectionId, out var queryManager))
            {
                throw new InvalidOperationException($"No QueryManager for connection '{Context.ConnectionId}'");
            }

            var queryResult = await queryManager.QueryAsync(query, cancellationToken);
            var channel = queryResult.Channel;
            return channel.Reader;
        }
    }

    public class QueryRequest
    {
    }

    public class QueryManager
    {
        public async Task CloseAllAsync()
        {
            
        }

        public async Task<QueryResult> QueryAsync(QueryRequest query, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class QueryResult
    {
        public Channel<ExecutionResult> Channel { get; }
    }
}