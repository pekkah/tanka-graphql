using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Server.Tests.Host;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Tanka.GraphQL.Server.Tests.WebSockets
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

        [Fact]
        public async Task Disconnect_with_dispose()
        {
            /* Given */
            /* When */
            var socket = await ConnectAsync();
            socket.Dispose();

            /* Then */
            Assert.Equal(WebSocketState.Closed, socket.State);
        }
    }
}