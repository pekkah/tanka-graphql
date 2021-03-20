using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Experimental.Tests
{
    public class ExperimentalSchemaAdapterFacts
    {
        [Fact]
        public async Task Execute()
        {
            /* Given */
            var schema = new ExecutableSchemaBuilder()
                .Add((TypeSystemDocument) @"
                    type Query {
                        hello: String!
                    }

                    schema {
                        query: Query
                    }
                ")
                .Add((TypeSystemDocument)@"
                    scalar String
                    ")
                .Build();

            ObjectTypeMap resolvers = new () 
            {
                ["Query"] = new()
                {
                    {"hello", context => ResolveSync.As("World!")}
                }
            };

            ExecutableDocument query = @"
                    {
                        hello
                    }";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Document = query,
                Schema = new ExperimentalSchemaAdapter(schema, resolvers, ScalarType.StandardConverters)
            });

            /* Then */
            result.ShouldMatchJson(@"{
                  ""data"": {
                    ""hello"": ""World!""
                  }
                }");
        }
    }
}