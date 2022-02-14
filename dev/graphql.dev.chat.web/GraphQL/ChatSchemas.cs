using Tanka.GraphQL.Extensions.Analysis;
using Tanka.GraphQL.Samples.Chat.Data;
using Tanka.GraphQL.Samples.Chat.Data.IDL;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Samples.Chat.Web.GraphQL;

public class ChatSchemas
{
    public ChatSchemas(IChatResolverService resolverService)
    {
        var builder = IdlSchema.Load();
        var resolvers = new ChatResolvers(resolverService);

        // add cost directive support to schema
        builder.Add(CostAnalyzer.CostDirective);

        // build  executable schema
        Chat = builder.Build(resolvers, resolvers).Result;
    }

    public ISchema Chat { get; set; }
}