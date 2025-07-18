using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Tanka.GraphQL.Server.WebSockets;

using Xunit;

namespace Tanka.GraphQL.Server.Tests.WebSockets;

public class SubscriptionErrorScenarioFacts : IAsyncDisposable
{
    private readonly TankaGraphQLServerFactory _factory = new();

    [Fact]
    public async Task Subscription_with_resolver_exception_should_send_error_message()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "error-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            errorEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));

        // Publish an event that will cause an error during resolution
        await _factory.Events.Publish(new ErrorEvent
        {
            Id = "error-event",
            ShouldThrow = true
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
        Assert.Contains("Test error", error.Payload[0].Message);
    }

    [Fact]
    public async Task Subscription_stream_cancellation_should_send_complete_message()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "cancellation-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            cancelableEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));

        // Trigger cancellation of the subscription stream
        await _factory.Events.Publish(new CancellableEvent
        {
            Id = "cancel-event",
            ShouldCancel = true
        });

        /* Then */
        var message = await webSocket.Receive();
        var complete = Assert.IsType<Complete>(message);
        Assert.Equal(subscriptionId, complete.Id);
    }

    [Fact]
    public async Task Multiple_subscriptions_with_one_failing_should_not_affect_others()
    {
        /* Given */
        var webSocket = await Connect(true);
        var workingId = "working-subscription";
        var failingId = "failing-subscription";

        /* When */
        // Create a working subscription
        await webSocket.Send(new Subscribe
        {
            Id = workingId,
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

        // Create a failing subscription
        await webSocket.Send(new Subscribe
        {
            Id = failingId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            errorEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForAtLeastSubscribers(TimeSpan.FromSeconds(30), 2);

        // Publish events that will cause one to fail and one to succeed
        await _factory.Events.Publish(new ErrorEvent
        {
            Id = "error-event",
            ShouldThrow = true
        });

        await _factory.Events.Publish(new MessageEvent
        {
            Id = "normal-event"
        });

        /* Then */
        var message1 = await webSocket.Receive();
        var message2 = await webSocket.Receive();

        // Should receive one error and one next message
        Assert.True(
            (message1 is Error error1 && error1.Id == failingId && message2 is Next next1 && next1.Id == workingId) ||
            (message1 is Next next2 && next2.Id == workingId && message2 is Error error2 && error2.Id == failingId));
    }

    [Fact]
    public async Task Subscription_with_invalid_field_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "invalid-field-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            nonExistentField 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
        Assert.Contains("nonExistentField", error.Payload[0].Message);
    }

    [Fact]
    public async Task Subscription_with_authorization_failure_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "auth-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            secureEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
        Assert.Contains("Unauthorized", error.Payload[0].Message);
    }

    [Fact]
    public async Task Subscription_with_variables_validation_error_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "variables-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription($requiredInput: String!) 
                        { 
                            eventsByInput(input: $requiredInput) 
                            { 
                                id 
                            } 
                        }
                        """,
                Variables = new Dictionary<string, object>
                {
                    // Missing required variable
                }
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
        Assert.Contains("requiredInput", error.Payload[0].Message);
    }

    [Fact]
    public async Task Subscription_with_operation_name_not_found_should_return_error()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "operation-name-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription EventsSubscription 
                        { 
                            events 
                            { 
                                id 
                            } 
                        }
                        
                        subscription OtherSubscription 
                        { 
                            events 
                            { 
                                message 
                            } 
                        }
                        """,
                OperationName = "NonExistentOperation"
            }
        });

        /* Then */
        var message = await webSocket.Receive();
        var error = Assert.IsType<Error>(message);
        Assert.Equal(subscriptionId, error.Id);
        Assert.NotEmpty(error.Payload);
        Assert.Contains("NonExistentOperation", error.Payload[0].Message);
    }

    [Fact]
    public async Task Subscription_timeout_should_send_complete_message()
    {
        /* Given */
        var webSocket = await Connect(true);
        var subscriptionId = "timeout-test";

        /* When */
        await webSocket.Send(new Subscribe
        {
            Id = subscriptionId,
            Payload = new GraphQLHttpRequest
            {
                Query = """
                        subscription 
                        { 
                            timeoutEvents 
                            { 
                                id 
                            } 
                        }
                        """
            }
        });

        await _factory.Events.WaitForSubscribers(TimeSpan.FromSeconds(30));

        // Publish an event that will cause a timeout
        await _factory.Events.Publish(new TimeoutEvent
        {
            Id = "timeout-event",
            ShouldTimeout = true
        });

        /* Then */
        var message = await webSocket.Receive();
        var complete = Assert.IsType<Complete>(message);
        Assert.Equal(subscriptionId, complete.Id);
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