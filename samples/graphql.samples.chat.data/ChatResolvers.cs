using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using fugu.graphql.resolvers;
using fugu.graphql.samples.chat.data.domain;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.chat.data
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(Chat chat)
        {
            Task<IResolveResult> GetChannels(ResolverContext context)
            {
                var channels = chat.Channels;
                return Task.FromResult(As(channels));
            }

            Task<ISubscribeResult> Subscribe(ResolverContext context)
            {
                var member = new Member()
                {
                    DisplayName = "todo",
                    Id = "todo"
                };

                var channel = chat.GetChannel("general"); //todo: name as input
                var stream = channel.Join(member);

                return Task.FromResult(Stream(stream, () =>
                {
                    channel.Leave(member);
                    stream.Complete();
                    return stream.Completion;
                }));
            }

            Task<IResolveResult> AddMessage(ResolverContext context)
            {
                var inputMessage = context.GetArgument<InputMessage>("message");
                var message = chat.AddReceivedMessage(inputMessage);

                return Task.FromResult(As(message));
            }

            this["Query"] = new FieldResolverMap()
            {
                {"channels", GetChannels}
            };

            this["Mutation"] = new FieldResolverMap()
            {
                {"addMessage", AddMessage}
            };

            this["Subscription"] = new FieldResolverMap()
            {              
                {"messageAdded", Subscribe, context => Task.FromResult(As(context.ObjectValue))}
            };

            this["Channel"] = new FieldResolverMap()
            {
                {"name", PropertyOf<Channel>(c => c.Name)},
                {"members", PropertyOf<Channel>(c => c.Members)},
                {"messages", PropertyOf<Channel>(chat.GetMessages)}
            };

            this["Member"] = new FieldResolverMap()
            {
                {"displayName", PropertyOf<Member>(m => m.DisplayName)},
                {"id", PropertyOf<Member>(m => m.Id)}
            };

            this["Message"] = new FieldResolverMap()
            {
                {"content", PropertyOf<Message>(m => m.Content)},
                {"from", PropertyOf<Message>(m => m.From)},
                {"sentAt", PropertyOf<Message>(m => m.SentAt)}
            };
        }
    }
}