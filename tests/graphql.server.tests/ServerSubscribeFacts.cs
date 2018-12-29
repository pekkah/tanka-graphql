using System;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerSubscribeFacts : ServerFactsBase
    {
        [Fact]
        public async Task Subscribe()
        {
            /* Given */
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("Query", new QueryRequest
            {
                Query = @"
subscription { 
    helloEvents
}"
            }, cancellationToken: cts.Token);

            await _eventManager.HelloAllAsync("world");

            /* Then */
            var result = await reader.ReadAsync();

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "helloEvents" && value.ToString() == "world";
            });

            cts.Cancel();
            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
        }

        public ServerSubscribeFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
    }
}