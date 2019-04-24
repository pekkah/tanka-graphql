using System.Collections.Generic;
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
            var actual = await Application.Clients.Single().Value.Input.ReadAsync();
            Assert.Equal(message, actual);
        }

        [Fact]
        public async Task ReceiveThreeMessages()
        {
            /* Given */
            using var socket = await ConnectAsync();
            const int messageCount = 3;
            var messages = new List<OperationMessage>();
            for (int i = 0; i < messageCount; i++)
            {
                var message = new OperationMessage
                {
                    Id = $"{i}",
                    Type = MessageType.GQL_CONNECTION_INIT
                };
                messages.Add(message);
            }

            /* When */
            for (int i = 0; i < messageCount; i++)
            {
                var bytes = SerializeMessage(messages[i]);
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }


            /* Then */
            for (int i = 0; i < 3; i++)
            {
                var actualMessage = await Application.Clients.Single().Value.Input.ReadAsync();
                Assert.Equal(messages[i], actualMessage);
            }
        }
    }
}