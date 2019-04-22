using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.server.tests.webSockets
{
    public class WebSocketServer_ReceiveMessageFacts : WebSocketFactsBase
    {
        public WebSocketServer_ReceiveMessageFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task ReceiveOneMessage()
        {
            /* Given */
            using var socket = await ConnectAsync();

            var message = new OperationMessage
            {
                Id = "1",
                Type = MessageType.GQL_CONNECTION_INIT
            };
            var bytes = SerializeMessage(message);

            /* When */
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

            /* Then */
            
        }
    }
}