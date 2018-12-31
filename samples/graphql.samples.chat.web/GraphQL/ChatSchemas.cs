using System.Threading.Tasks;
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
            var schema = FromIdlAsync().Result;        
            var resolvers = new ChatResolvers(resolverService);

            Chat = SchemaTools.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers,
                resolvers).Result;
        }

        public Task<ISchema> FromIdlAsync()
        {
            return IdlSchema.CreateAsync();
        }

        public ISchema Chat { get; set; }
    }
}