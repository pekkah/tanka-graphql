using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerSubscribeToLotsOfEvents : ServerFactsBase
    {
        [Fact]
        public async Task Subscribe_to_lots_of_events()
        {
            /* Given */
            int count = 10_000;
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            var hubConnection = Connect();
            await hubConnection.StartAsync();

            /* When */
            var reader = await hubConnection.StreamAsChannelAsync<ExecutionResult>("Query", new QueryRequest
            {
                Query = @"
subscription { 
    helloEvents(id:""003"")
}"
            }, cancellationToken: cts.Token);

            for(int i = 0; i < count;i++)
            {
                await _eventManager.HelloAllAsync(i.ToString());
            }
            

            /* Then */
            var results = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var result = await reader.ReadAsync(cts.Token);

                results.Add(result.Data["helloEvents"].ToString());
            }

            for (int i = 0; i < count; i++)
            {
                Assert.Contains(i.ToString(), results);
            }

            cts.Cancel();
            await hubConnection.StopAsync();
        }

        public ServerSubscribeToLotsOfEvents(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
    }
}