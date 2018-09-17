using fugu.graphql.samples.chat.data.domain;
using static fugu.graphql.resolvers.Resolve;

namespace fugu.graphql.samples.chat.data
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(IMessageResolverService resolverService)
        {
            this["Query"] = new FieldResolverMap
            {
                {"messages", context => resolverService.GetMessagesAsync()}
            };

            this["Mutation"] = new FieldResolverMap()
            {
                {"addMessage", resolverService.AddMessageAsync}
            };

            this["Message"] = new FieldResolverMap()
            {
                {"id", PropertyOf<Message>(m => m.Id)},
                {"from", PropertyOf<Message>(m => m.From)},
                {"content", PropertyOf<Message>(m => m.Content)},
                {"timestamp", PropertyOf<Message>(m => m.Timestamp)}
            };

            this["From"] = new FieldResolverMap()
            {
                {"userId", PropertyOf<From>(f => f.UserId)},
                {"name", PropertyOf<From>(f => f.Name)}
            };
        }
    }
}