using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using graphql.server.tests.host;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.server.tests.webSockets
{
    public class WebSocketConnection_ConnectionFacts : WebSocketFactsBase
    {
        public WebSocketConnection_ConnectionFacts(WebApplicationFactory<Startup> factory) : base(factory)
        {
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
        public async Task Disconnect()
        {
            /* Given */
            /* When */
            using var socket = await ConnectAsync();
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);

            /* Then */
            Assert.Equal(WebSocketState.Closed, socket.State);
        }
    }
}