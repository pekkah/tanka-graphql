using System.Threading.Tasks;
using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.data.domain;
using fugu.graphql.samples.chat.data.idl;
using fugu.graphql.type;

namespace fugu.graphql.server.tests.subscriptions.specs
{
    public class ChatFixture
    {
        public Chat Chat { get; }

        public ChatFixture()
        {
            Chat = new Chat();
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var schema = await IdlSchema.CreateAsync();
            var resolvers = new ChatResolvers(Chat);

            return await SchemaTools.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers,
                resolvers);
        }
    }
}