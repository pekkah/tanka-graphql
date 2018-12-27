using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.resolvers;
using fugu.graphql.tests.data;
using fugu.graphql.tools;
using fugu.graphql.type;
using Xunit;
using static fugu.graphql.Executor;
using static fugu.graphql.Parser;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.tests
{
    public class Message
    {
        public string Content { get; set; }
    }

    public class SubscriptionsFacts
    {
        public SubscriptionsFacts()
        {
            // schema
            var messageType = new ObjectType(
                "Message",
                new Fields
                {
                    ["content"] = new Field(ScalarType.String)
                });

            var messageListType = new List(messageType);
            var schema = new Schema(
                new ObjectType(
                    "Query",
                    new Fields
                    {
                        ["messages"] = new Field(messageListType)
                    }),
                null,
                new ObjectType(
                    "Subscription",
                    new Fields
                    {
                        ["messageAdded"] = new Field(messageType)
                    }));


            // data
            var messages = new List<Message>();
            var channels = new List<Channel<object>>();

            // resolvers
            Task<IResolveResult> GetMessagesAsync(ResolverContext context)
            {
                return Task.FromResult(As(messages));
            }

            async Task<ISubscribeResult> OnMessageAdded(ResolverContext context, CancellationToken cancellationToken)
            {
                var channel = Channel.CreateUnbounded<object>();
                channels.Add(channel);
                cancellationToken.Register(() =>
                {
                    channel.Writer.Complete();
                    channels.Remove(channel);
                });

                foreach (var message in messages)
                {
                    await channel.Writer.WriteAsync(message);
                }

                // return result
                return AsReader(channel.Reader);
            }

            Task<IResolveResult> ResolveMessage(ResolverContext context)
            {
                return Task.FromResult(As(context.ObjectValue));
            }

            var resolvers = new ResolverMap
            {
                ["Query"] = new FieldResolverMap
                {
                    {"messages", GetMessagesAsync}
                },
                ["Subscription"] = new FieldResolverMap
                {
                    {"messageAdded", OnMessageAdded, ResolveMessage}
                },
                ["Message"] = new FieldResolverMap
                {
                    {"content", PropertyOf<Message>(r => r.Content)}
                }
            };

            // make executable
            _executable = SchemaTools.MakeExecutableSchemaAsync(
                schema,
                resolvers,
                resolvers).Result;

            _postMessage = async (Message message) =>
            {
                messages.Add(message);
                foreach (var channel in channels)
                {
                    await channel.Writer.WriteAsync(message);
                }
            };
        }

        private readonly ISchema _executable;
        private readonly Func<Message, Task> _postMessage;

        [Fact]
        public async Task Should_stream_a_lot()
        {
            /* Given */
            const int count = 10_000;
            var unsubscribe = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            for (var i = 0; i < count; i++)
            {
                var expected = new Message {Content = i.ToString()};
                await _postMessage(expected);
            }

            var query = @"
subscription MessageAdded {
    messageAdded {
        content
    }
}
";

            /* When */
            var result = await SubscribeAsync(new ExecutionOptions
            {
                Document = ParseDocument(query),
                Schema = _executable
            }, unsubscribe.Token).ConfigureAwait(false);

            /* Then */
            for (var i = 0; i < count; i++)
            {
                var actualResult = await result.Reader.ReadAsync(unsubscribe.Token).ConfigureAwait(false);

                actualResult.ShouldMatchJson(@"{
    ""data"":{
        ""messageAdded"": {
            ""content"": ""{counter}""
        }
    }
}".Replace("{counter}", i.ToString()));
            }

            unsubscribe.Cancel();
        }

        [Fact]
        public async Task Should_subscribe()
        {
            /* Given */
            var unsubscribe = new CancellationTokenSource();
            var expected = new Message {Content = "hello"};
            await _postMessage(expected);

            var query = @"
subscription MessageAdded {
    messageAdded {
        content
    }
}
";

            /* When */
            var result = await SubscribeAsync(new ExecutionOptions
            {
                Document = ParseDocument(query),
                Schema = _executable
            }, unsubscribe.Token).ConfigureAwait(false);

            /* Then */
            var actualResult = await result.Reader.ReadAsync().ConfigureAwait(false);
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
}