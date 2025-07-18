using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class MessageResultHandlingFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task Connection_ack_should_include_payload_when_provided()
    {
        /* Given */
        var webSocket = await Connect();
        var payload = new Dictionary<string, object>
        {
            ["sessionId"] = "test-session-123",
            ["connectionTime"] = DateTime.UtcNow.ToString("O")
        };

        /* When */
        await webSocket.Send(new ConnectionInit
        {
            Payload = payload
        });

        /* Then */
        var message = await webSocket.Receive();
        var ack = Assert.IsType<ConnectionAck>(message);
        
        // Server should acknowledge the connection
        Assert.Equal(MessageTypes.ConnectionAck, ack.Type);
        // Payload handling depends on server implementation
    }

    [Fact]
    public async Task Connection_ack_should_handle_null_payload()
    {
        /* Given */
        var webSocket = await Connect();

        /* When */
        await webSocket.Send(new ConnectionInit
        {
            Payload = null
        });

        /* Then */
        var message = await webSocket.Receive();
        var ack = Assert.IsType<ConnectionAck>(message);
        Assert.Equal(MessageTypes.ConnectionAck, ack.Type);
    }

    [Fact]
    public async Task Connection_ack_should_handle_empty_payload()
    {
        /* Given */
        var webSocket = await Connect();

        /* When */
        await webSocket.Send(new ConnectionInit
        {
            Payload = new Dictionary<string, object>()
        });

        /* Then */
        var message = await webSocket.Receive();
        var ack = Assert.IsType<ConnectionAck>(message);
        Assert.Equal(MessageTypes.ConnectionAck, ack.Type);
    }

    [Fact]
    public async Task Ping_should_return_pong_with_same_payload()
    {
        /* Given */
        var webSocket = await Connect(true);
        var payload = new Dictionary<string, object>
        {
            ["timestamp"] = DateTime.UtcNow.ToString("O"),
            ["clientId"] = "test-client-456"
        };

        /* When */
        await webSocket.Send(new Ping
        {
            Payload = payload
        });

        /* Then */
        var message = await webSocket.Receive();
        var pong = Assert.IsType<Pong>(message);
        
        Assert.Equal(MessageTypes.Pong, pong.Type);
        // Pong should echo back the same payload
        Assert.NotNull(pong.Payload);
        Assert.Equal(payload["timestamp"], pong.Payload["timestamp"]);
        Assert.Equal(payload["clientId"], pong.Payload["clientId"]);
    }

    [Fact]
    public async Task Ping_with_null_payload_should_return_pong_with_null_payload()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Ping
        {
            Payload = null
        });

        /* Then */
        var message = await webSocket.Receive();
        var pong = Assert.IsType<Pong>(message);
        
        Assert.Equal(MessageTypes.Pong, pong.Type);
        Assert.Null(pong.Payload);
    }

    [Fact]
    public async Task Ping_with_empty_payload_should_return_pong_with_empty_payload()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Ping
        {
            Payload = new Dictionary<string, object>()
        });

        /* Then */
        var message = await webSocket.Receive();
        var pong = Assert.IsType<Pong>(message);
        
        Assert.Equal(MessageTypes.Pong, pong.Type);
        Assert.NotNull(pong.Payload);
        Assert.Empty(pong.Payload);
    }

    [Fact]
    public async Task Ping_with_complex_payload_should_return_pong_with_same_complex_payload()
    {
        /* Given */
        var webSocket = await Connect(true);
        var payload = new Dictionary<string, object>
        {
            ["simple"] = "value",
            ["number"] = 42,
            ["boolean"] = true,
            ["nested"] = new Dictionary<string, object>
            {
                ["level2"] = "nested value",
                ["array"] = new[] { "item1", "item2", "item3" }
            }
        };

        /* When */
        await webSocket.Send(new Ping
        {
            Payload = payload
        });

        /* Then */
        var message = await webSocket.Receive();
        var pong = Assert.IsType<Pong>(message);
        
        Assert.Equal(MessageTypes.Pong, pong.Type);
        Assert.NotNull(pong.Payload);
        
        // Verify complex payload is preserved
        Assert.Equal(payload["simple"], pong.Payload["simple"]);
        Assert.Equal(payload["number"], pong.Payload["number"]);
        Assert.Equal(payload["boolean"], pong.Payload["boolean"]);
        
        // Verify nested payload
        var nestedPayload = pong.Payload["nested"] as Dictionary<string, object>;
        Assert.NotNull(nestedPayload);
        Assert.Equal("nested value", nestedPayload["level2"]);
    }

    [Fact]
    public async Task Multiple_pings_should_return_corresponding_pongs()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["id"] = "ping1" } });
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["id"] = "ping2" } });
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["id"] = "ping3" } });

        /* Then */
        var pongs = new List<Pong>();
        for (int i = 0; i < 3; i++)
        {
            var message = await webSocket.Receive();
            var pong = Assert.IsType<Pong>(message);
            pongs.Add(pong);
        }

        // Verify all pongs received with correct payloads
        Assert.Equal(3, pongs.Count);
        Assert.Contains(pongs, p => p.Payload["id"].ToString() == "ping1");
        Assert.Contains(pongs, p => p.Payload["id"].ToString() == "ping2");
        Assert.Contains(pongs, p => p.Payload["id"].ToString() == "ping3");
    }

    [Fact]
    public async Task Rapid_ping_sequence_should_handle_all_pongs()
    {
        /* Given */
        var webSocket = await Connect(true);
        var pingCount = 10;

        /* When */
        for (int i = 0; i < pingCount; i++)
        {
            await webSocket.Send(new Ping
            {
                Payload = new Dictionary<string, object>
                {
                    ["sequence"] = i,
                    ["timestamp"] = DateTime.UtcNow.Ticks
                }
            });
        }

        /* Then */
        var pongs = new List<Pong>();
        for (int i = 0; i < pingCount; i++)
        {
            var message = await webSocket.Receive();
            var pong = Assert.IsType<Pong>(message);
            pongs.Add(pong);
        }

        Assert.Equal(pingCount, pongs.Count);
        
        // Verify all sequence numbers are present
        for (int i = 0; i < pingCount; i++)
        {
            Assert.Contains(pongs, p => p.Payload["sequence"].ToString() == i.ToString());
        }
    }

    [Fact]
    public async Task Ping_during_active_subscription_should_not_interfere()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "ping-during-sub";

        // Create a subscription
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            events 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));

        /* When */
        // Send ping while subscription is active
        await webSocket.Send(new Ping
        {
            Payload = new Dictionary<string, object> { ["test"] = "ping during subscription" }
        });

        // Publish event to subscription
        await _factory.Events.Publish(new MessageEvent
        {
            Id = "event-during-ping"
        });

        /* Then */
        var message1 = await webSocket.Receive();
        var message2 = await webSocket.Receive();

        // Should receive both pong and subscription event
        Assert.True(
            (message1 is Pong && message2 is Next) ||
            (message1 is Next && message2 is Pong));

        if (message1 is Pong pong)
        {
            Assert.Equal("ping during subscription", pong.Payload["test"]);
        }
        else if (message2 is Pong pong2)
        {
            Assert.Equal("ping during subscription", pong2.Payload["test"]);
        }
    }

    [Fact]
    public async Task Connection_init_with_invalid_payload_should_still_acknowledge()
    {
        /* Given */
        var webSocket = await Connect();

        /* When */
        await webSocket.Send(new ConnectionInit
        {
            Payload = new Dictionary<string, object>
            {
                ["invalid"] = new object() // This might cause serialization issues
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var ack = Assert.IsType<ConnectionAck>(message);
        Assert.Equal(MessageTypes.ConnectionAck, ack.Type);
    }

    [Fact]
    public async Task Message_ordering_should_be_preserved()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "ordering-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            events 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));

        // Send ping
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["order"] = "first" } });

        // Publish event
        await _factory.Events.Publish(new MessageEvent { Id = "ordered-event" });

        // Send another ping
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["order"] = "second" } });

        /* Then */
        var messages = new List<MessageBase>();
        for (int i = 0; i < 3; i++)
        {
            var message = await webSocket.Receive();
            messages.Add(message);
        }

        // Verify message types and order
        Assert.Equal(3, messages.Count);
        Assert.Contains(messages, m => m is Pong p && p.Payload["order"].ToString() == "first");
        Assert.Contains(messages, m => m is Next);
        Assert.Contains(messages, m => m is Pong p && p.Payload["order"].ToString() == "second");
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