using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace fugu.graphql.server
{
    public class ServerHub : Hub<IServerClient>
    {
        public async Task Initialize(string id)
        {
            await Clients.Caller.ConnectionAck(id);
        }
    }

    public interface IServerClient
    {
        Task ConnectionAck(string id);
    }
}