using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
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
    private readonly BroadcastChannel<Message> _messageBroadcast;
    private readonly Channel<Message> _messageChannel;

    public SubscriptionsFacts()
    {
        // data
        var messages = new List<Message>();
        _messageChannel = Channel.CreateUnbounded<Message>();
        _messageBroadcast = new BroadcastChannel<Message>(_messageChannel);
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
            context.ResolvedValue = _messageBroadcast.Subscribe(unsubscribe);
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

    [Fact]
    public async Task Should_stream_a_lot()
    {
        /* Given */
        const int count = 10_000;
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

        for (var i = 0; i < count; i++)
        {
            var expected = new Message { Content = i.ToString() };
            await _messageChannel.Writer.WriteAsync(expected);
        }

        /* Then */
        var readCount = 0;
        for (var i = 0; i < count; i++)
        {
            await result.MoveNextAsync();
            var actualResult = result.Current;

            actualResult.ShouldMatchJson(@"{
                    ""data"":{
                        ""messageAdded"": {
                            ""content"": ""{counter}""
                        }
                    }
                }".Replace("{counter}", i.ToString()));

            readCount++;
        }

        Assert.Equal(count, readCount);
        unsubscribe.Cancel();
    }

    [Fact]
    public async Task Should_subscribe()
    {
        /* Given */
        var unsubscribe = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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

        await _messageChannel.Writer.WriteAsync(expected);

        /* Then */
        await result.MoveNextAsync();
        var actualResult = result.Current;
        unsubscribe.Cancel();

        actualResult.ShouldMatchJson(@"{
                ""data"":{
                    ""messageAdded"": {
                        ""content"": ""hello""
                    }
                }
            }");
    }
}