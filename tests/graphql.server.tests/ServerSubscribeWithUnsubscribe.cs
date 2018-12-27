using System;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerSubscribeWithUnsubscribe : ServerFactsBase
    {
        [Fact]
        public async Task Subscribe_with_unsubscribe()
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
    helloEvents(id: ""002"")
}"
            }, cancellationToken: cts.Token);

            await _eventManager.HelloAllAsync("world");

            /* Then */
            var result = await reader.ReadAsync(cts.Token);

            Assert.Contains(result.Data, kv =>
            {
                var (key, value) = kv;
                return key == "helloEvents" && value.ToString() == "world";
            });

            cts.Cancel();

            await hubConnection.StopAsync();
        }

        public ServerSubscribeWithUnsubscribe(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
    }
}