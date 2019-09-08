using tanka.graphql.samples.chat.data.domain;
using static tanka.graphql.resolvers.Resolve;

namespace tanka.graphql.samples.chat.data
{
    public class ChatResolvers : TypeMap
    {
        public ChatResolvers(IChatResolverService resolverService)
        {
            this["Query"] = new FieldResolversMap
            {
                {"messages", resolverService.GetMessagesAsync}
            };

            this["Mutation"] = new FieldResolversMap()
            {
                {"addMessage", resolverService.AddMessageAsync},
                {"editMessage", resolverService.EditMessageAsync}
            };

            this["Subscription"] = new FieldResolversMap
            {
                {"messages", resolverService.StreamMessagesAsync, resolverService.ResolveMessageAsync}
            };

            this["Message"] = new FieldResolversMap()
            {
                {"id", PropertyOf<Message>(m => m.Id)},
                {"from", PropertyOf<Message>(m => m.From)},
                {"content", PropertyOf<Message>(m => m.Content)},
                {"timestamp", PropertyOf<Message>(m => m.Timestamp)}
            };

            this["From"] = new FieldResolversMap()
            {
                {"userId", PropertyOf<From>(f => f.UserId)},
                {"name", PropertyOf<From>(f => f.Name)}
            };
        }
    }
}