using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tanka.graphql.introspection;
using tanka.graphql.schema;
using tanka.graphql.sdl;
using Xunit;

namespace tanka.graphql.tests.bug
{
    public class Bug_339
    {
        [Fact]
        public async Task Introspection_should_pass()
        {
            /* Given */
            var schema = new SchemaBuilder()
                .Sdl(@"
                        input InputItem {
                          foo: [String]
                        }

                        type Mutation {
                          createItem(input: InputItem!): ID
                        }

                        type Query {
                            field : String
                        }

                        schema {
                            query: Query
                            mutation: Mutation
                        }
                ")
                .Build();

            var introspectionSchema = Introspect.Schema(schema);

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = introspectionSchema,
                Document = Parser.ParseDocument(Introspect.DefaultQuery)
            });

            /* Then */
            var types = (List<object>)result.Select("__schema", "types");

            foreach (var type in types)
            {
                var typeDictionary = (Dictionary<string, object>) type;

                if ((string) typeDictionary["name"] == "InputItem")
                {
                    var inputFields = (List<object>)typeDictionary["inputFields"];

                    foreach (var inputField in inputFields)
                    {
                        var inputFieldDictionary = (Dictionary<string, object>) inputField;

                        if ((string) inputFieldDictionary["name"] == "foo")
                        {
                            var defaultValue = inputFieldDictionary["defaultValue"];
                            Assert.Null(defaultValue);
                        }
                    }
                }
            }
        }
    }
}