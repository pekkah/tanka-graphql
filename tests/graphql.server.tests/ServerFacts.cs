using System;
using System.Net.Http;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerFacts : IClassFixture<WebApplicationFactory<Startup>>
    {
        public ServerFacts(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _server = factory.Server;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(_server.BaseAddress, "graphql"),
                    o => { o.HttpMessageHandlerFactory = _ => _server.CreateHandler(); })
                .Build();
        }

        private readonly HubConnection _hubConnection;
        private readonly TestServer _server;
        private HttpClient _client;

        [Fact]
        public async Task Connect_and_receive_ack()
        {
            /* Given */
            var cts = new TaskCompletionSource<string>();

            await _hubConnection.StartAsync();
            _hubConnection.Closed += exception =>
            {
                if (exception != null)
                    cts.SetException(exception);
                
                return Task.CompletedTask;
            };

            _hubConnection.On<string>("ConnectionAck", id =>
            {
                cts.SetResult(id);
            });

            /* When */
            await _hubConnection.SendAsync("Initialize", "1");

            /* Then */
            await Task.WhenAny(cts.Task, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.True(cts.Task.IsCompleted);
            Assert.Equal("1", cts.Task.Result);
            await _hubConnection.StopAsync();
        }
    }
}