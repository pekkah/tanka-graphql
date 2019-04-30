using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using tanka.graphql.channels;
using tanka.graphql.requests;

namespace tanka.graphql.server
{
    public class ServerHub : Hub
    {
        private readonly IQueryStreamService _queryStreamService;

        public ServerHub(IQueryStreamService queryStreamService)
        {
            _queryStreamService = queryStreamService;
        }

        [HubMethodName("query")]
        public ChannelReader<ExecutionResult> QueryAsync(
            QueryRequest query,
            CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<ExecutionResult>();
            var _ = Task.Run(async ()=>
            {
                var result = await _queryStreamService.QueryAsync(query, cancellationToken);
                var __ = result.Reader.LinkTo(channel.Writer);
            }, CancellationToken.None);
            return channel.Reader;
        }
    }
}