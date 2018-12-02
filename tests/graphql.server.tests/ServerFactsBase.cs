using System;
using System.Net.Http;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace fugu.graphql.server.tests
{
    public class ServerFactsBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private TestServer _server;
        private HttpClient _client;
        protected EventManager _eventManager;

        public ServerFactsBase(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _server = factory.Server;
            _eventManager = factory.Server.Host.Services.GetRequiredService<EventManager>();
        }

        protected HubConnection Connect()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_server.BaseAddress, "graphql"),
                    o => { o.HttpMessageHandlerFactory = _ => _server.CreateHandler(); })
                .Build();

            connection.Closed += exception =>
            {
                Assert.Null(exception);
                return Task.CompletedTask;
            };

            return connection;
        }
    }
}