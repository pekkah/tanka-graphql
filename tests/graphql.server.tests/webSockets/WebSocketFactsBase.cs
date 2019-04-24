using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

namespace tanka.graphql.server.tests.webSockets
{
    public abstract class WebSocketFactsBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient Client;

        protected WebApplicationFactory<Startup> Factory;

        protected WebSocketFactsBase(WebApplicationFactory<Startup> factory)
        {
            Sink = new MessageSinkProtocol();
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => { services.AddSingleton<IProtocolHandler>(Sink); });
            });
            Client = Factory.CreateClient();
            Application = Factory.Server.Host.Services.GetRequiredService<WebSocketServer>();
        }

        public MessageSinkProtocol Sink { get; set; }

        public WebSocketServer Application { get; set; }

        protected async Task<WebSocket> ConnectAsync()
        {
            var webSocketClient = Factory.Server.CreateWebSocketClient();
            webSocketClient.ConfigureRequest = request =>
            {
                request.Headers.Add("Sec-WebSocket-Protocol", "graphql-ws");
            };
            return await webSocketClient.ConnectAsync(new Uri("http://localhost/graphql"), CancellationToken.None);
        }

        protected OperationMessage DeserializeMessage(string json)
        {
            return JsonConvert.DeserializeObject<OperationMessage>(json);
        }

        protected byte[] SerializeMessage(OperationMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}