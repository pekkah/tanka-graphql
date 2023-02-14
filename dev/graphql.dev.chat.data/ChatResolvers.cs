using Tanka.GraphQL.Samples.Chat.Data.Domain;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data;

public static class ChatSchemaConfigurationExtensions
{
    public static ExecutableSchemaBuilder AddChat(this ExecutableSchemaBuilder builder)
    {
        builder.ConfigureObject("Query", new()
        {
            { "messages: [Message!]!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().GetMessagesAsync(r)) }
        });

        builder.ConfigureObject("Mutation", new()
        {
            { "addMessage(message: InputMessage!): Message!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().AddMessageAsync(r)) },
            { "editMessage(id: String!, message: InputMessage!): Message", b => b.Run(r => r.GetRequiredService<IChatResolverService>().EditMessageAsync(r)) }
        });

        builder.ConfigureObject("Subscription", new()
        {
            { "messages: Message!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().ResolveMessageAsync(r)) }
        }, new()
        {
            { "messages: Message!", b => b.Run((r, ct) => r.GetRequiredService<IChatResolverService>().StreamMessagesAsync(r, ct)) }
        });

        builder.ConfigureObject("Message", new()
        {
            { "id: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Id) },
            { "from: From!", context => context.ResolveAsPropertyOf<Message>(m => m.From) },
            { "content: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Content) },
            { "timestamp: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Timestamp) }
        });

        builder.ConfigureObject("From", new()
        {
            { "userId: String!", context => context.ResolveAsPropertyOf<From>(f => f.UserId) },
            { "name: String!", context => context.ResolveAsPropertyOf<From>(f => f.Name) }
        });

        builder.AddTypeSystem("""
            input InputMessage {
                content: String!
            }
            """);

        return builder;
    }
}