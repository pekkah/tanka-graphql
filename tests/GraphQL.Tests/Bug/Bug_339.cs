using System.Collections.Generic;
using System.Threading.Tasks;

using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests.Bug;

public class Bug_339
{
    [Fact]
    public async Task Introspection_should_pass()
    {
        /* Given */
        var schema = await new SchemaBuilder()
            .Add(@"
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
            .Build(new());


        /* When */
        var result = await Executor.Execute(schema, Introspect.DefaultQuery);

        /* Then */
        var types = (List<object>)result.Select("__schema", "types");

        foreach (var type in types)
        {
            var typeDictionary = (Dictionary<string, object>)type;

            if ((string)typeDictionary["name"] == "InputItem")
            {
                var inputFields = (List<object>)typeDictionary["inputFields"];

                foreach (var inputField in inputFields)
                {
                    var inputFieldDictionary = (Dictionary<string, object>)inputField;

                    if ((string)inputFieldDictionary["name"] == "foo")
                    {
                        var defaultValue = inputFieldDictionary["defaultValue"];
                        Assert.Null(defaultValue);
                    }
                }
            }
        }
    }
}