using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Server.Links.DTOs;

namespace Tanka.GraphQL.Server;

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
        var _ = Task.Run(async () =>
        {
            var result = await _queryStreamService.QueryAsync(new Query
            {
                Document = query.Query,
                OperationName = query.OperationName,
                Extensions = query.Extensions,
                Variables = query.Variables
            }, cancellationToken);
            var __ = result.Reader.WriteTo(channel.Writer);
        }, CancellationToken.None);
        return channel.Reader;
    }
}