using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class EchoFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task DirectEchoMultipleFacts()
    {
        /* Given */
        var webSocket = await Connect();


        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "1",
            Payload = new GraphQLHttpRequest
            {
                Query = "query { hello }"
            }
        });

        await webSocket.Send(new Subscribe
        {
            Id = "2",
            Payload = new GraphQLHttpRequest
            {
                Query = "query { hello }"
            }
        });


        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));
        var message2 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
        Assert.IsType<Subscribe>(message2);
    }

    [Fact]
    public async Task DirectEchoMultipleAlternativeFacts()
    {
        /* Given */
        var webSocket = await Connect();


        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "1",
            Payload = new GraphQLHttpRequest
            {
                Query = "query { hello }"
            }
        });
        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        await webSocket.Send(new Subscribe
        {
            Id = "2",
            Payload = new GraphQLHttpRequest
            {
                Query = "query { hello }"
            }
        });
        var message2 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
        Assert.IsType<Subscribe>(message2);
    }

    [Fact]
    public async Task DirectEchoFacts()
    {
        /* Given */
        var webSocket = await Connect(EchoProtocol.Protocol);


        /* When */
        await webSocket.Send(new Subscribe()
        {
            Id = "1",
            Payload = new GraphQLHttpRequest()
            {
                Query = "query { hello }"
            }
        });

        var message1 = await webSocket.Receive(TimeSpan.FromSeconds(360));

        /* Then */
        Assert.IsType<Subscribe>(message1);
    }

    private async Task<WebSocket> Connect(string protocol = EchoProtocol.Protocol)
    {
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add(protocol);
        var webSocket = await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);

        return webSocket;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory != null) await _factory.DisposeAsync();
    }
}