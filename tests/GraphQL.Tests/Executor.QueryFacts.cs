using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class QueryFacts
{
    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new ()
            {
                { "version: String!", b => b.ResolveAs("1.0") }
            })
            .Build();

        ExecutableDocument query = """
            {
                version
            }
            """;

        /* When */
        var result = await new Executor(schema)
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
            .Add("Query", new()
            {
                { "system: System!", b => b.ResolveAs("System") }
            })
            .Build();

        ExecutableDocument query = """
            {
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

    [Fact]
    public async Task Execute_With_Variables()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new ()
            {
                { "hello(name: String!): String!", (string name) => $"Hello, {name}!" }
            })
            .Build();

        ExecutableDocument query = """
            query($name: String!) {
                hello(name: $name)
            }
            """;

        var request = new GraphQLRequest
        {
            Query = query,
            Variables = new Dictionary<string, object?>
            {
                { "name", "World" }
            }
        };

        /* When */
        var result = await new Executor(schema)
            .Execute(request);

        /* Then */
        result.ShouldMatchJson("""
            {
                "data": {
                    "hello": "Hello, World!"
                }
            }
            """);
    }

    [Fact]
    public async Task Execute_With_OperationName()
    {
        /* Given */
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new ()
            {
                { "hello: String!", b => b.ResolveAs("Hello, World!") },
                { "goodbye: String!", b => b.ResolveAs("Goodbye, World!") }
            })
            .Build();

        ExecutableDocument query = """
            query GetHello {
                hello
            }

            query GetGoodbye {
                goodbye
            }
            """;

        var request = new GraphQLRequest
        {
            Query = query,
            OperationName = "GetHello"
        };

        /* When */
        var result = await new Executor(schema)
            .Execute(request);

        /* Then */
        result.ShouldMatchJson("""
            {
                "data": {
                    "hello": "Hello, World!"
                }
            }
            """);
    }
}
