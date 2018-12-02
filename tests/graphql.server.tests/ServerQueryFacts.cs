using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerQueryFacts : ServerFactsBase
    {
        public ServerQueryFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Query()
        {
            /* Given */
            var cts = new CancellationTokenSource();
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                    Query = "{ hello }"
            }, cancellationToken: cts.Token);

            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            await hubConnection.StopAsync();
        }

        [Fact]
        public async Task Multiple_Queries()
        {
            /* Given */
            var cts = new CancellationTokenSource();
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader1 = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                Query = "{ hello }"
            }, cancellationToken: cts.Token);

            var reader2 = await hubConnection.StreamAsChannelAsync<ExecutionResult>("query", new QueryRequest
            {
                Query = "{ hello }"
            }, cancellationToken: cts.Token);

            /* Then */
            var result1 = await reader1.ReadAsync();
            var result2 = await reader2.ReadAsync();

            Assert.Contains(result1.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            Assert.Contains(result2.Data, kv =>
            {
                var (key, value) = kv;
                return key == "hello" && value.ToString() == "world";
            });

            await hubConnection.StopAsync();
        }
    }
}