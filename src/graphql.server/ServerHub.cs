using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerHub : Hub
    {
        private readonly ServerClients _clients;

        public ServerHub(ServerClients clients)
        {
            _clients = clients;
        }

        public override Task OnConnectedAsync()
        {
            return _clients.OnConnectedAsync(Context);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return _clients.OnDisconnectedAsync(Context, exception);
        }

        [HubMethodName("query")]
        public async Task<ChannelReader<ExecutionResult>> QueryAsync(
            QueryRequest query,
            CancellationToken cancellationToken)
        {
            var queryResult = await _clients.QueryAsync(Context, query, cancellationToken);
            var channel = queryResult.Channel;
            return channel.Reader;
        }
    }
}