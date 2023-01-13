using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Tests.Experimental;

public class MutationFacts
{
    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .ConfigureObject("Query", new ()
            {
                {"version: String!", b => b.ResolveAs("1.0")}
            })
            .Build();

        ExecutableDocument query = """
            mutation {
                version
            }
            """;

        /* When */
        var result = await new GraphQL.Experimental.Executor(schema)
            .ExecuteAsync(new GraphQLRequest()
            {
                Document = query
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
            .ConfigureObject("System", new ()
            {
                {"version: String!", b => b.ResolveAs("1.0")}
            })
            .ConfigureObject("Query", new ()
            {
                {"system: System!", b => b.ResolveAs("System")}
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
        var result = await new GraphQL.Experimental.Executor(schema)
            .ExecuteAsync(new GraphQLRequest()
            {
                Document = query
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