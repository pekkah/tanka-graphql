using System.Collections.Generic;
using System.Threading.Tasks;
using fugu.graphql.samples.chat.data;
using fugu.graphql.samples.chat.data.domain;
using fugu.graphql.samples.chat.data.idl;
using fugu.graphql.tools;
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