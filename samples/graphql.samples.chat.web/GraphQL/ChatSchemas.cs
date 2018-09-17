using System.Threading.Tasks;
using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.data.idl;
using fugu.graphql.tools;
using fugu.graphql.type;

namespace fugu.graphql.samples.chat.web.GraphQL
{
    public class ChatSchemas
    {
        public ChatSchemas(IMessageResolverService resolverService)
        {
            var schema = FromIdlAsync().Result;        
            var resolvers = new ChatResolvers(resolverService);

            Chat = SchemaTools.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers).Result;
        }

        public Task<ISchema> FromIdlAsync()
        {
            return IdlSchema.CreateAsync();
        }

        public ISchema Chat { get; set; }
    }
}