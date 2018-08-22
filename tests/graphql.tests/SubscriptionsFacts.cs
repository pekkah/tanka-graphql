using System.Collections.Generic;
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
using static fugu.graphql.type.ScalarType;

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
                    ["content"] = new Field(String)
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
            _messagesChannel = new BufferBlock<Message>();

            // resolvers
            Task<IResolveResult> GetMessagesAsync(ResolverContext context)
            {
                return Task.FromResult(As(messages));
            }

            async Task<ISubscribeResult> OnMessageAdded(ResolverContext context)
            {
                var reader = new BufferBlock<Message>();
                _messagesChannel.LinkTo(reader, new DataflowLinkOptions()
                {
                    PropagateCompletion = true
                });
                
                // noop
                await Task.Delay(0).ConfigureAwait(false);

                // return result
                return Stream(reader, () =>
                {
                    reader.Complete();
                    return reader.Completion;
                });
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
                ["Message"] = new FieldResolverMap()
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
        public async Task Should_subscribe()
        {
            /* Given */
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
            }).ConfigureAwait(false);

            /* Then */
            var actualResult = await result.Source.ReceiveAsync().ConfigureAwait(false);
            await result.UnsubscribeAsync().ConfigureAwait(false);

            actualResult.ShouldMatchJson(@"{
    ""errors"": null,
    ""data"":{
        ""messageAdded"": {
            ""content"": ""hello""
        }
    }
}");
        }
    }
}