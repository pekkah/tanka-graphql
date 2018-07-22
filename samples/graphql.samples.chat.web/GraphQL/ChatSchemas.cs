using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.data.domain;
using fugu.graphql.samples.chat.data.idl;
using fugu.graphql.samples.chat.data.schema;
using fugu.graphql.type;

namespace fugu.graphql.samples.chat.web.GraphQL
{
    public class ChatSchemas
    {
        public ChatSchemas(Chat chat)
        {
            var schema = FromIdlAsync().Result;
            
            //var schema = ModelSchema.CreateAsync().Result;

            var resolvers = new ChatResolvers(chat);
            Chat = ExecutableSchema.MakeExecutableSchemaWithIntrospection(
                schema,
                resolvers).Result;
        }

        public Task<ISchema> FromIdlAsync()
        {
            return IdlSchema.CreateAsync();
        }

        public Task<ISchema> FromModelAsync()
        {
            return ModelSchema.CreateAsync();
        }

        public ExecutableSchema Chat { get; set; }

        private IEnumerable<KeyValuePair<string, IField>> ResolveFieldConflict(ComplexType left,
            ComplexType right, KeyValuePair<string, IField> conflict)
        {
            return new[]
            {
                conflict
            };
        }
    }
}