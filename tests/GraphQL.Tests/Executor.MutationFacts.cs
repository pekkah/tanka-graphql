using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class MutationFacts
{
    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .Add("Mutation", new ()
            {
                { "version: String!", b => b.ResolveAs("1.0") }
            })
            .Build();

        ExecutableDocument query = """
            mutation {
                version
            }
            """;

        /* When */
        var result = await new GraphQL.Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = query
            });

        /* Then */
        result.ShouldMatchJson("""
            {
               "data": {
                  "version": "1.0"
              }
            }
            """);
    }

    [Fact]
    public async Task Object_with_ScalarField()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .Add("System", new ()
            {
                { "version: String!", b => b.ResolveAs("1.0") }
            })
            .Add("Mutation", new ()
            {
                { "system: System!", b => b.ResolveAs("System") }
            })
            .Build();

        ExecutableDocument query = """
            mutation {
                system {
                    version
                }
            }
            """;

        /* When */
        var result = await new GraphQL.Executor(schema)
            .Execute(new GraphQLRequest
            {
                Query = query
            });

        /* Then */
        result.ShouldMatchJson("""
            {
              "data": {
                "system": {
                  "version": "1.0"
                }
              }
            }
            """);
    }
}
