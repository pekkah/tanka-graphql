using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql.resolvers;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;
using static tanka.graphql.Executor;
using static tanka.graphql.Parser;
using static tanka.graphql.resolvers.Resolve;

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
            _messagesChannel = new BufferBlock<Message>();

            // resolvers
            Task<IResolveResult> GetMessagesAsync(ResolverContext context)
            {
                return Task.FromResult(As(messages));
            }

            async Task<ISubscribeResult> OnMessageAdded(ResolverContext context, CancellationToken cancellationToken)
            {
                var reader = new BufferBlock<Message>();
                var sub = _messagesChannel.LinkTo(reader, new DataflowLinkOptions
                {
                    PropagateCompletion = true
                });

                cancellationToken.Register(() => sub.Dispose());

                // noop
                await Task.Delay(0).ConfigureAwait(false);

                // return result
                return Stream(reader);
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
        }

        private readonly ISchema _executable;
        private readonly BufferBlock<Message> _messagesChannel;

        [Fact]
        public async Task Should_stream_a_lot()
        {
            /* Given */
            const int count = 10_000;
            var unsubscribe = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            for (var i = 0; i < count; i++)
            {
                var expected = new Message {Content = i.ToString()};
                await _messagesChannel.SendAsync(expected).ConfigureAwait(false);
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
                var actualResult = await result.Source.ReceiveAsync(unsubscribe.Token).ConfigureAwait(false);

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
            await _messagesChannel.SendAsync(expected).ConfigureAwait(false);

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
            var actualResult = await result.Source.ReceiveAsync().ConfigureAwait(false);
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