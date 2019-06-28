using System.Threading.Tasks;
using tanka.graphql.extensions.analysis;
using tanka.graphql.samples.chat.data;
using tanka.graphql.samples.chat.data.idl;
using tanka.graphql.tools;
using tanka.graphql.type;

namespace tanka.graphql.samples.chat.web.GraphQL
{
    public class ChatSchemas
    {
        public ChatSchemas(IChatResolverService resolverService)
        {
            var builder = IdlSchema.Load();       
            var resolvers = new ChatResolvers(resolverService);

            // add cost directive support to schema
            builder.IncludeDirective(CostAnalyzer.CostDirective);

            // build  executable schema
            Chat = SchemaTools.MakeExecutableSchemaWithIntrospection(
                builder,
                resolvers,
                resolvers);
        }
        
        public ISchema Chat { get; set; }
    }
}