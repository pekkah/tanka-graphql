using System;
using System.Collections.Generic;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Samples.Chat.Data.Domain;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Samples.Chat.Data;

public static class ChatSchemaConfigurationExtensions
{
    public static ExecutableSchemaBuilder AddChat(this ExecutableSchemaBuilder builder)
    {
        builder.Object("Query", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
        {
            { "messages: [Message!]!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().GetMessagesAsync(r)) }
        });

        builder.Object("Mutation", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
        {
            { "addMessage(message: InputMessage!): Message!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().AddMessageAsync(r)) },
            { "editMessage(id: String!, message: InputMessage!): Message", b => b.Run(r => r.GetRequiredService<IChatResolverService>().EditMessageAsync(r)) }
        });

        builder.Object("Subscription", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
        {
            { "messages: Message!", b => b.Run(r => r.GetRequiredService<IChatResolverService>().ResolveMessageAsync(r)) }
        }, new()
        {
            { "messages: Message!", b => b.Run((r, ct) => r.GetRequiredService<IChatResolverService>().StreamMessagesAsync(r, ct)) }
        });

        builder.Object("Message", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
        {
            { "id: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Id) },
            { "from: From!", context => context.ResolveAsPropertyOf<Message>(m => m.From) },
            { "content: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Content) },
            { "timestamp: String!", context => context.ResolveAsPropertyOf<Message>(m => m.Timestamp) }
        });

        builder.Object("From", new Dictionary<FieldDefinition, Action<ResolverBuilder>>()
        {
            { "userId: String!", context => context.ResolveAsPropertyOf<From>(f => f.UserId) },
            { "name: String!", context => context.ResolveAsPropertyOf<From>(f => f.Name) }
        });

        builder.Add("""
            input InputMessage {
                content: String!
            }
            """);

        return builder;
    }
}