using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class Message
{
    public string Content { get; set; }
}

public class SubscriptionsFacts
{
    private readonly ISchema _executable;
    private readonly EventAggregator<Message> _eventAggregator;

    public SubscriptionsFacts()
    {
        // data
        var messages = new List<Message>();
        _eventAggregator = new EventAggregator<Message>();
        // schema
        var builder = new SchemaBuilder()
            .Add(@"
type Message {
    content: String
}

type Query {
    messages: [Message]
}

type Subscription {
    messageAdded: Message
}

");

        // resolvers
        ValueTask GetMessagesAsync(ResolverContext context)
        {
            return context.ResolveAs(messages);
        }

        ValueTask OnMessageAdded(SubscriberContext context, CancellationToken unsubscribe)
        {
            context.ResolvedValue = _eventAggregator.Subscribe(unsubscribe);
            return default;
        }

        ValueTask ResolveMessage(ResolverContext context)
        {
            return context.ResolveAs(context.ObjectValue);
        }

        var resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "messages", GetMessagesAsync }
            },
            ["Subscription"] = new()
            {
                { "messageAdded", OnMessageAdded, ResolveMessage }
            },
            ["Message"] = new()
            {
                { "content", context => context.ResolveAsPropertyOf<Message>(r => r.Content) }
            }
        };

        // make executable
        _executable = builder.Build(resolvers, resolvers).Result;
    }

    [Fact(Skip = "flaky")]
    public async Task Should_stream_a_lot()
    {
        /* Given */
        const int count = 1000;
        var unsubscribe = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        var query = @"
                subscription MessageAdded {
                    messageAdded {
                        content
                    }
                }
                ";

        /* When */ 
        await using var result = Executor.Subscribe(_executable, query, unsubscribe.Token)
            .GetAsyncEnumerator(unsubscribe.Token);

        var initialMoveNext = result.MoveNextAsync();

        await _eventAggregator.WaitForSubscribers(TimeSpan.FromSeconds(15));

        for (var i = 0; i < count; i++)
        {
            var expected = new Message { Content = i.ToString() };
            await _eventAggregator.Publish(expected);
        }

        /* Then */
        await initialMoveNext;
        var readCount = 0;
        for (var i = 0; i < count; i++)
        {
            var actualResult = result.Current;

            actualResult.ShouldMatchJson(@"{
                    ""data"":{
                        ""messageAdded"": {
                            ""content"": ""{counter}""
                        }
                    }
                }".Replace("{counter}", i.ToString()));

            readCount++;

            await result.MoveNextAsync();
        }

        Assert.Equal(count, readCount);
        unsubscribe.Cancel();
    }

    [Fact]
    public async Task Should_subscribe()
    {
        /* Given */
        using var unsubscribe = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var expected = new Message { Content = "hello" };

        var query = @"
                subscription MessageAdded {
                    messageAdded {
                        content
                    }
                }
                ";

        /* When */
        await using var result = Executor.Subscribe(_executable, query, unsubscribe.Token)
            .GetAsyncEnumerator(unsubscribe.Token);

        var initial = result.MoveNextAsync();
        await _eventAggregator.Publish(expected);
        await initial;
        
        /* Then */
        var actualResult = result.Current;
        await unsubscribe.CancelAsync();

        actualResult.ShouldMatchJson(@"{
                ""data"":{
                    ""messageAdded"": {
                        ""content"": ""hello""
                    }
                }
            }");
    }

    [Fact]
    public async Task Should_error_on_invalid_query()
    {
        /* Given */
        var unsubscribe = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var query = @"
                subscription MessageAdded {
                    messageAdded {
                        doesNotExists
                    }
                }
                ";

        await using var result = Executor.Subscribe(_executable, query, unsubscribe.Token)
            .GetAsyncEnumerator(unsubscribe.Token);

        /* When */
        /* Then */
        await Assert.ThrowsAsync<ValidationException>(()=> result.MoveNextAsync().AsTask());
    }
}