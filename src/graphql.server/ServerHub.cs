using System;
using System.Threading.Tasks;
using fugu.graphql.server.subscriptions;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerHub : Hub<IServerClient>
    {
        private readonly SubscriptionServerManager _servers;

        public ServerHub(SubscriptionServerManager servers)
        {
            _servers = servers;
        }

        public override Task OnConnectedAsync()
        {
            _servers.OnConnected(Context.ConnectionId, Clients.Caller);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _servers.OnDisconnected(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public Task Start(Request request)
        {
            request.Type = MessageType.GQL_START;
            return _servers.Execute(Context.ConnectionId, request);
        }

        public Task Stop(string id)
        {
            return _servers.Stop(Context.ConnectionId, id);
        }

    }

    public interface IServerClient
    {
        Task Data(OperationMessage message);
    }
}