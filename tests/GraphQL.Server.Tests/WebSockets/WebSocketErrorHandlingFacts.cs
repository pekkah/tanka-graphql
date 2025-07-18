using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Tanka.GraphQL.Server.WebSockets;
using Tanka.GraphQL.Server.WebSockets.Results;

using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class WebSocketErrorHandlingFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task Connect_with_invalid_protocol_should_fail()
    {
        /* Given */
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add("invalid-protocol");

        /* When & Then */
        await Assert.ThrowsAsync<WebSocketException>(async () =>
        {
            await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);
        });
    }

    [Fact]
    public async Task Connect_with_unsupported_protocol_should_fail()
    {
        /* Given */
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add("graphql-ws-legacy");

        /* When & Then */
        await Assert.ThrowsAsync<WebSocketException>(async () =>
        {
            await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);
        });
    }

    [Fact]
    public async Task Send_malformed_json_should_close_connection()
    {
        /* Given */
        var webSocket = await Connect();
        var malformedJson = "{invalid json"u8;

        /* When */
        await webSocket.SendAsync(
            malformedJson,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.UnsupportedData, result.CloseStatus);
    }

    [Fact]
    public async Task Send_message_without_type_should_close_connection()
    {
        /* Given */
        var webSocket = await Connect();
        var invalidMessage = """{"id": "1", "payload": {}}"""u8;

        /* When */
        await webSocket.SendAsync(
            invalidMessage,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.UnsupportedData, result.CloseStatus);
    }

    [Fact]
    public async Task Send_message_with_invalid_type_should_return_unknown_message_result()
    {
        /* Given */
        var webSocket = await Connect(true);
        var invalidMessage = """{"type": "invalid_type", "id": "1"}"""u8;

        /* When */
        await webSocket.SendAsync(
            invalidMessage,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.UnsupportedData, result.CloseStatus);
    }

    [Fact]
    public async Task Send_subscribe_without_connection_init_should_close_connection()
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

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.PolicyViolation, result.CloseStatus);
    }

    [Fact]
    public async Task Send_duplicate_connection_init_should_close_connection()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new ConnectionInit());

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.PolicyViolation, result.CloseStatus);
    }

    [Fact]
    public async Task Send_subscribe_with_invalid_query_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "test-id";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = "invalid query syntax {"
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
    }

    [Fact]
    public async Task Send_subscribe_with_null_query_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "test-id";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = null
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
    }

    [Fact]
    public async Task Send_subscribe_with_empty_query_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "test-id";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = ""
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
    }

    [Fact]
    public async Task Send_complete_for_nonexistent_subscription_should_not_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var nonExistentId = "non-existent-id";

        /* When */
        await webSocket.Send(new Complete
        {
            Id = nonExistentId
        });

        /* Then */
        // Should not receive any message or error - completing a non-existent subscription is safe
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await webSocket.Receive(cts.Token);
        });
    }

    [Fact]
    public async Task Send_binary_message_should_close_connection()
    {
        /* Given */
        var webSocket = await Connect();
        var binaryData = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        /* When */
        await webSocket.SendAsync(
            binaryData,
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.UnsupportedData, result.CloseStatus);
    }

    private async Task<WebSocket> Connect(bool connectionInit = false, string protocol = GraphQLWSTransport.GraphQLTransportWSProtocol)
    {
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add(protocol);
        var webSocket = await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);

        if (connectionInit)
        {
            await webSocket.Send(new ConnectionInit());
            using var cancelReceive = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var ack = await webSocket.Receive(cancelReceive.Token);
            Assert.IsType<ConnectionAck>(ack);
        }

        return webSocket;
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory != null) await _factory.DisposeAsync();
    }
}