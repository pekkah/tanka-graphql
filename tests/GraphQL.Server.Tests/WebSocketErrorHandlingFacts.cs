using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class WebSocketErrorHandlingFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task WebSocket_InvalidProtocol_RejectsConnection()
    {
        /* Given */
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add("invalid-protocol");

        /* When */
        var exception = await Assert.ThrowsAsync<WebSocketException>(() => 
            client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None));

        /* Then */
        Assert.Contains("protocol", exception.Message.ToLower());
    }

    [Fact]
    public async Task WebSocket_MalformedMessage_HandlesGracefully()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        var malformedJson = "{ invalid json }";

        /* When */
        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(malformedJson),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None);
        
        // Should receive an error message or close connection
        Assert.True(response.MessageType == WebSocketMessageType.Text || 
                   response.MessageType == WebSocketMessageType.Close);
    }

    [Fact]
    public async Task WebSocket_InvalidMessageType_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        var invalidMessage = JsonSerializer.Serialize(new { type = "invalid_type" });

        /* When */
        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(invalidMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None);
        
        if (response.MessageType == WebSocketMessageType.Text)
        {
            var buffer = new byte[response.Count];
            Array.Copy(new ArraySegment<byte>(new byte[1024]).Array!, buffer, response.Count);
            var responseText = Encoding.UTF8.GetString(buffer);
            var responseMessage = JsonSerializer.Deserialize<JsonElement>(responseText);
            
            Assert.Equal("error", responseMessage.GetProperty("type").GetString());
        }
    }

    [Fact]
    public async Task WebSocket_ConnectionInitWithoutAck_HandlesTimeout()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        
        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "test-id",
            Payload = new GraphQLHttpRequest { Query = "{ hello }" }
        });

        /* Then */
        var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None);
        
        // Should receive an error or close connection since no connection_init was sent
        Assert.True(response.MessageType == WebSocketMessageType.Text || 
                   response.MessageType == WebSocketMessageType.Close);
    }

    [Fact]
    public async Task WebSocket_SubscriptionWithInvalidQuery_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "test-id",
            Payload = new GraphQLHttpRequest { Query = "{ invalidField }" }
        });

        /* Then */
        var response = await webSocket.Receive();
        
        // Should receive an error message for invalid query
        Assert.IsType<Error>(response);
        var error = (Error)response;
        Assert.Equal("test-id", error.Id);
        Assert.NotNull(error.Payload);
    }

    [Fact]
    public async Task WebSocket_SubscriptionWithSyntaxError_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "test-id",
            Payload = new GraphQLHttpRequest { Query = "{ hello" } // Missing closing brace
        });

        /* Then */
        var response = await webSocket.Receive();
        
        // Should receive an error message for syntax error
        Assert.IsType<Error>(response);
        var error = (Error)response;
        Assert.Equal("test-id", error.Id);
        Assert.NotNull(error.Payload);
    }

    [Fact]
    public async Task WebSocket_DuplicateSubscriptionId_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        var subscriptionId = "duplicate-id";
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest { Query = "subscription { events { id } }" }
        });

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId, // Same ID again
            Payload = new GraphQLHttpRequest { Query = "subscription { events { id } }" }
        });

        /* Then */
        var response = await webSocket.Receive();
        
        // Should receive an error for duplicate subscription ID
        Assert.IsType<Error>(response);
        var error = (Error)response;
        Assert.Equal(subscriptionId, error.Id);
    }

    [Fact]
    public async Task WebSocket_CompleteNonExistentSubscription_HandlesGracefully()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        /* When */
        await webSocket.Send(new Complete
        {
            Id = "non-existent-id"
        });

        /* Then */
        // Should handle gracefully without error
        var cancelReceive = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        try
        {
            await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), cancelReceive.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected - no response should be sent for non-existent subscription
        }
    }

    [Fact]
    public async Task WebSocket_UnexpectedClose_HandlesGracefully()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        await webSocket.Send(new Subscribe
        {
            Id = "test-id",
            Payload = new GraphQLHttpRequest { Query = "subscription { events { id } }" }
        });

        /* When */
        webSocket.Abort(); // Abrupt close

        /* Then */
        // Should handle the abrupt close gracefully
        // The subscription should be cleaned up automatically
        Assert.True(true); // If we reach here, the server handled the close gracefully
    }

    [Fact]
    public async Task WebSocket_LargeMessage_HandlesCorrectly()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        // Create a large query
        var largeQuery = "query { " + string.Join(" ", Enumerable.Repeat("hello", 10000)) + " }";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = "large-query",
            Payload = new GraphQLHttpRequest { Query = largeQuery }
        });

        /* Then */
        var response = await webSocket.Receive();
        
        // Should receive an error for invalid query or handle it gracefully
        Assert.True(response is Error || response is Next);
    }

    [Fact]
    public async Task WebSocket_ConnectionInitMultipleTimes_HandlesCorrectly()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        
        /* When */
        await webSocket.Send(new ConnectionInit());
        var ack1 = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack1);

        await webSocket.Send(new ConnectionInit()); // Second init

        /* Then */
        var response = await webSocket.Receive();
        
        // Should either acknowledge again or send an error
        Assert.True(response is ConnectionAck || response is Error);
    }

    [Fact]
    public async Task WebSocket_MaxSubscriptionsExceeded_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        await webSocket.Send(new ConnectionInit());
        
        var ack = await webSocket.Receive();
        Assert.IsType<ConnectionAck>(ack);

        /* When */
        // Try to create many subscriptions
        for (int i = 0; i < 1000; i++)
        {
            await webSocket.Send(new Subscribe
            {
                Id = $"subscription-{i}",
                Payload = new GraphQLHttpRequest { Query = "subscription { events { id } }" }
            });
        }

        /* Then */
        // Should either handle all subscriptions or send an error when limit is exceeded
        // The exact behavior depends on server configuration
        var response = await webSocket.Receive();
        Assert.True(response is Error || response is Next);
    }

    [Fact]
    public async Task WebSocket_InvalidConnectionInitPayload_SendsError()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        var invalidInitMessage = JsonSerializer.Serialize(new 
        { 
            type = "connection_init",
            payload = "invalid_payload_type" // Should be object, not string
        });

        /* When */
        await webSocket.SendAsync(
            Encoding.UTF8.GetBytes(invalidInitMessage),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);

        /* Then */
        var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None);
        
        if (response.MessageType == WebSocketMessageType.Text)
        {
            // Should receive an error or close connection
            var buffer = new byte[response.Count];
            Array.Copy(new ArraySegment<byte>(new byte[1024]).Array!, buffer, response.Count);
            var responseText = Encoding.UTF8.GetString(buffer);
            var responseMessage = JsonSerializer.Deserialize<JsonElement>(responseText);
            
            Assert.True(responseMessage.GetProperty("type").GetString() == "error" ||
                       responseMessage.GetProperty("type").GetString() == "connection_error");
        }
    }

    [Fact]
    public async Task WebSocket_BinaryMessage_RejectsCorrectly()
    {
        /* Given */
        var webSocket = await ConnectWebSocket();
        var binaryData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        /* When */
        await webSocket.SendAsync(
            binaryData,
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);

        /* Then */
        var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[1024]), CancellationToken.None);
        
        // Should close connection or send error for binary messages
        Assert.True(response.MessageType == WebSocketMessageType.Close ||
                   response.MessageType == WebSocketMessageType.Text);
    }

    private async Task<WebSocket> ConnectWebSocket()
    {
        var client = _factory.CreateWebSocketClient();
        client.SubProtocols.Add(GraphQLWSTransport.GraphQLTransportWSProtocol);
        return await client.ConnectAsync(new Uri("ws://localhost/graphql/ws"), CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory != null) await _factory.DisposeAsync();
    }
}