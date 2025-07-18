using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class ConnectionLifecycleFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task Abrupt_connection_termination_should_cleanup_subscriptions()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "cleanup-test";

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
        Assert.Equal(1, _factory.Events.SubscriberCount);

        /* When */
        // Abruptly close the connection
        webSocket.Abort();

        /* Then */
        await _factory.Events.WaitForNoSubscribers(TimeSpan.FromSeconds(30));
        Assert.Equal(0, _factory.Events.SubscriberCount);
    }

    [Fact]
    public async Task Connection_close_during_subscription_should_cleanup_resources()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "close-test";

        // Create multiple subscriptions
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId + "1",
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

        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId + "2",
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

        await _factory.Events.WaitForAtLeastSubscribers(TimeSpan.FromSeconds(30), 2);
        Assert.Equal(2, _factory.Events.SubscriberCount);

        /* When */
        // Close the connection normally
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);

        /* Then */
        await _factory.Events.WaitForNoSubscribers(TimeSpan.FromSeconds(30));
        Assert.Equal(0, _factory.Events.SubscriberCount);
    }

    [Fact]
    public async Task Connection_timeout_should_close_with_appropriate_status()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        // Send a message that will cause server to timeout
        await webSocket.Send(new Subscribe
        {
            Id = "timeout-test",
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            slowEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        // Wait for timeout to occur
        await Task.Delay(TimeSpan.FromSeconds(35));

        /* Then */
        var buffer = new ArraySegment<byte>(new byte[1024]);
        var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        
        Assert.NotNull(result.CloseStatus);
        Assert.Equal(WebSocketCloseStatus.InternalServerError, result.CloseStatus);
    }

    [Fact]
    public async Task Multiple_clients_with_one_disconnecting_should_not_affect_others()
    {
        /* Given */
        var webSocket1 = await Connect(true);
        var webSocket2 = await Connect(true);
        
        var subscriptionId1 = "client1-sub";
        var subscriptionId2 = "client2-sub";

        // Create subscriptions from both clients
        await webSocket1.Send(new Subscribe
        {
            Id = subscriptionId1,
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

        await webSocket2.Send(new Subscribe
        {
            Id = subscriptionId2,
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

        await _factory.Events.WaitForAtLeastSubscribers(TimeSpan.FromSeconds(30), 2);
        Assert.Equal(2, _factory.Events.SubscriberCount);

        /* When */
        // Disconnect first client
        webSocket1.Abort();

        // Wait for cleanup
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Publish event
        await _factory.Events.Publish(new MessageEvent
        {
            Id = "test-event"
        });

        /* Then */
        // Second client should still receive the event
        var message = await webSocket2.Receive();
        var next = Assert.IsType<Next>(message);
        Assert.Equal(subscriptionId2, next.Id);

        // Only one subscriber should remain
        Assert.Equal(1, _factory.Events.SubscriberCount);

        // Cleanup
        await webSocket2.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Connection_keepalive_ping_should_receive_pong()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Ping());

        /* Then */
        var message = await webSocket.Receive();
        var pong = Assert.IsType<Pong>(message);
        Assert.Equal(MessageTypes.Pong, pong.Type);
    }

    [Fact]
    public async Task Connection_keepalive_multiple_pings_should_receive_multiple_pongs()
    {
        /* Given */
        var webSocket = await Connect(true);

        /* When */
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["test"] = "ping1" } });
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["test"] = "ping2" } });
        await webSocket.Send(new Ping { Payload = new Dictionary<string, object> { ["test"] = "ping3" } });

        /* Then */
        var message1 = await webSocket.Receive();
        var message2 = await webSocket.Receive();
        var message3 = await webSocket.Receive();

        Assert.IsType<Pong>(message1);
        Assert.IsType<Pong>(message2);
        Assert.IsType<Pong>(message3);
    }

    [Fact]
    public async Task Connection_with_large_number_of_subscriptions_should_handle_gracefully()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionCount = 50;

        /* When */
        for (int i = 0; i < subscriptionCount; i++)
        {
            await webSocket.Send(new Subscribe
            {
                Id = $"sub-{i}",
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
        }

        await _factory.Events.WaitForAtLeastSubscribers(TimeSpan.FromSeconds(30), subscriptionCount);

        /* Then */
        Assert.Equal(subscriptionCount, _factory.Events.SubscriberCount);

        // Publish event and verify all subscriptions receive it
        await _factory.Events.Publish(new MessageEvent
        {
            Id = "bulk-test-event"
        });

        // Should receive messages from all subscriptions
        var receivedMessages = 0;
        var timeout = TimeSpan.FromSeconds(30);
        var deadline = DateTime.UtcNow.Add(timeout);

        while (receivedMessages < subscriptionCount && DateTime.UtcNow < deadline)
        {
            try
            {
                var message = await webSocket.Receive(TimeSpan.FromSeconds(1));
                if (message is Next)
                {
                    receivedMessages++;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Assert.Equal(subscriptionCount, receivedMessages);

        // Cleanup
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
    }

    [Fact]
    public async Task Connection_with_slow_client_should_handle_backpressure()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "backpressure-test";

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
        // Publish many events rapidly
        for (int i = 0; i < 10; i++)
        {
            await _factory.Events.Publish(new MessageEvent
            {
                Id = $"rapid-event-{i}"
            });
        }

        /* Then */
        // Should receive all events eventually without connection closing
        var receivedMessages = 0;
        var timeout = TimeSpan.FromSeconds(30);
        var deadline = DateTime.UtcNow.Add(timeout);

        while (receivedMessages < 10 && DateTime.UtcNow < deadline)
        {
            try
            {
                var message = await webSocket.Receive(TimeSpan.FromSeconds(1));
                if (message is Next)
                {
                    receivedMessages++;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Assert.Equal(10, receivedMessages);
        Assert.Equal(WebSocketState.Open, webSocket.State);

        // Cleanup
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
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