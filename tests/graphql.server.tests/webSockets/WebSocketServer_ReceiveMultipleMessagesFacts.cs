using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using tanka.graphql.server.webSockets.dtos;
using Xunit;

namespace tanka.graphql.server.tests.webSockets
{
    public class WebSocketServer_ReceiveMultipleMessagesFacts : WebSocketFactsBase
    {
        public WebSocketServer_ReceiveMultipleMessagesFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task ReceiveThreeMessages()
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
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

            /* Then */
            for (int i = 0; i < 3; i++)
            {
                //var messageJson = await Application.Messages.Reader.ReadAsync();
                //Assert.Equal(message, DeserializeMessage(messageJson));
            }
        }
    }
}