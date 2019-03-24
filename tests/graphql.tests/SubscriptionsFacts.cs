using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tanka.graphql.channels;
using tanka.graphql.resolvers;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests
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
            var builder = new SchemaBuilder();
            builder.Object("Message", out var messageType)
                .Connections(connect => connect
                    .Field(messageType, "content", ScalarType.String));

            var messageListType = new List(messageType);

            builder.Query(out var query)
                .Connections(connect => connect
                    .Field(query, "messages", messageListType));

            builder.Subscription(out var subscription)
                .Connections(connect => connect
                    .Field(subscription, "messageAdded", messageType));

            var schema = builder.Build();

            // data
            var messages = new List<Message>();
            _messagesChannel = new EventChannel<Message>();

            // resolvers
            ValueTask<IResolveResult> GetMessagesAsync(ResolverContext context)
            {
                return ResolveSync.As(messages);
            }

            ValueTask<ISubscribeResult> OnMessageAdded(ResolverContext context, CancellationToken unsubscribe)
            {
                return ResolveSync.Subscribe(_messagesChannel, unsubscribe);
            }

            ValueTask<IResolveResult> ResolveMessage(ResolverContext context)
            {
                return ResolveSync.As(context.ObjectValue);
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
                    {"content", Resolve.PropertyOf<Message>(r => r.Content)}
                }
            };

            // make executable
            _executable = SchemaTools.MakeExecutableSchema(
                schema,
                resolvers,
                resolvers);
        }

        private readonly ISchema _executable;
        private readonly EventChannel<Message> _messagesChannel;

        [Fact]
        public async Task Should_stream_a_lot()
        {
            /* Given */
            const int count = 10_000;
            var unsubscribe = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            var query = @"
                subscription MessageAdded {
                    messageAdded {
                        content
                    }
                }
                ";

            /* When */
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(query),
                Schema = _executable
            }, unsubscribe.Token).ConfigureAwait(false);

            for (var i = 0; i < count; i++)
            {
                var expected = new Message {Content = i.ToString()};
                await _messagesChannel.WriteAsync(expected);
            }

            /* Then */
            for (var i = 0; i < count; i++)
            {
                var actualResult = await result.Source.Reader.ReadAsync(unsubscribe.Token).ConfigureAwait(false);

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

            var query = @"
                subscription MessageAdded {
                    messageAdded {
                        content
                    }
                }
                ";

            /* When */
            var result = await Executor.SubscribeAsync(new ExecutionOptions
            {
                Document = Parser.ParseDocument(query),
                Schema = _executable
            }, unsubscribe.Token).ConfigureAwait(false);

            await _messagesChannel.WriteAsync(expected);

            /* Then */
            var actualResult = await result.Source.Reader.ReadAsync(unsubscribe.Token).ConfigureAwait(false);
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