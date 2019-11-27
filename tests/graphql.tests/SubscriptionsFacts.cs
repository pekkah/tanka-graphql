using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests
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
            ValueTask<IResolverResult> GetMessagesAsync(IResolverContext context)
            {
                return ResolveSync.As(messages);
            }

            ValueTask<ISubscriberResult> OnMessageAdded(IResolverContext context, CancellationToken unsubscribe)
            {
                return ResolveSync.Subscribe(_messagesChannel, unsubscribe);
            }

            ValueTask<IResolverResult> ResolveMessage(IResolverContext context)
            {
                return ResolveSync.As(context.ObjectValue);
            }

            var resolvers = new ObjectTypeMap
            {
                ["Query"] = new FieldResolversMap
                {
                    {"messages", GetMessagesAsync}
                },
                ["Subscription"] = new FieldResolversMap
                {
                    {"messageAdded", OnMessageAdded, ResolveMessage}
                },
                ["Message"] = new FieldResolversMap
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
            var unsubscribe = new CancellationTokenSource(TimeSpan.FromMinutes(1));

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
            var unsubscribe = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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