using System.Threading.Tasks;
using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.data.domain;
using fugu.graphql.samples.chat.data.idl;

namespace fugu.graphql.server.tests.subscriptions.specs
{
    public class ChatFixture
    {
        public Chat Chat { get; }

        public ChatFixture()
        {
            Chat = new Chat();
        }

        public async Task<ExecutableSchema> GetSchemaAsync()
        {
            var schema = await IdlSchema.CreateAsync();
            var resolvers = new ChatResolvers(Chat);

            return await ExecutableSchema.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers,
                resolvers);
        }
    }
}