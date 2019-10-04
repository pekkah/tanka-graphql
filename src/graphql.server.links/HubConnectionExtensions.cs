using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Tanka.GraphQL.Server.Links.DTOs;

namespace Tanka.GraphQL.Server.Links
{
    public static class HubConnectionExtensions
    {
        public static async Task<ChannelReader<ExecutionResult>> StreamQueryAsync(
            this HubConnection connection,
            QueryRequest query,
            CancellationToken cancellationToken)
        {
            var channel = await connection.StreamAsChannelAsync<ExecutionResult>(
                "query",
                query,
                cancellationToken);

            return channel;
        }
    }
}