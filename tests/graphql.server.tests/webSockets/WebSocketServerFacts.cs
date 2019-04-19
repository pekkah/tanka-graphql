using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using tanka.graphql.server.webSockets;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

namespace tanka.graphql.server.tests.webSockets
{
    public class WebSocketServerFacts : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _httpClient;
        private MessageServer _application;
        private TestServer _server;
        private WebSocketClient _webSocketClient;

        public WebSocketServerFacts(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
            _server = factory.Server;
            _webSocketClient = factory.Server.CreateWebSocketClient();
            
            _application = factory.Server.Host.Services.GetRequiredService<MessageServer>();
        }

        private Task<WebSocket> ConnectAsync(string protocol = "graphql-ws")
        {
            var client = _server.CreateWebSocketClient();
            client.ConfigureRequest = request => { request.Headers.Add("Sec-WebSocket-Protocol", protocol); };
            return client.ConnectAsync(new Uri("http://localhost/graphql"), CancellationToken.None);
        }

        [Fact]
        public async Task Connect()
        {
            /* Given */
            /* When */
            using var socket = await ConnectAsync();

            /* Then */
            Assert.Equal(WebSocketState.Open, socket.State);
        }

        [Fact]
        public async Task Send_Init()
        {
            /* Given */
            using var socket = await ConnectAsync();

            var message = new OperationMessage()
            {
                Id = "1",
                Type = MessageType.GQL_CONNECTION_INIT
            };
            var bytes = SerializeMessage(message);

            /* When */
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

            /* Then */
            var messageJson = await _application.Messages.Reader.ReadAsync();
            Assert.Equal(message, JsonConvert.DeserializeObject<OperationMessage>(messageJson));
        }

        private byte[] SerializeMessage(OperationMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}