using tanka.graphql.samples.chat.data.domain;
using static tanka.graphql.resolvers.Resolve;

namespace tanka.graphql.samples.chat.data
{
    public class ChatResolvers : ResolverMap
    {
        public ChatResolvers(IChatResolverService resolverService)
        {
            this["Query"] = new FieldResolverMap
            {
                {"messages", resolverService.GetMessagesAsync}
            };

            this["Mutation"] = new FieldResolverMap()
            {
                {"addMessage", resolverService.AddMessageAsync},
                {"editMessage", resolverService.EditMessageAsync}
            };

            this["Subscription"] = new FieldResolverMap
            {
                {"messages", resolverService.StreamMessagesAsync, resolverService.ResolveMessageAsync}
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