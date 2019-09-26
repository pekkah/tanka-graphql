using tanka.graphql.extensions.analysis;
using Tanka.GraphQL.Samples.Chat.Data;
using Tanka.GraphQL.Samples.Chat.Data.IDL;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace Tanka.GraphQL.Samples.Chat.Web.GraphQL
{
    public class ChatSchemas
    {
        public ChatSchemas(IChatResolverService resolverService)
        {
            var builder = IdlSchema.Load();
            var resolvers = new ChatResolvers(resolverService);

            // add cost directive support to schema
            builder.Include(CostAnalyzer.CostDirective);

            // build  executable schema
            Chat = SchemaTools.MakeExecutableSchemaWithIntrospection(
                builder,
                resolvers,
                resolvers);
        }

        public ISchema Chat { get; set; }
    }
}