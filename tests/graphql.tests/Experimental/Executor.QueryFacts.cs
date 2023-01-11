using System.Threading.Tasks;
using Tanka.GraphQL.Experimental;
using Tanka.GraphQL.Language.Nodes;
using Xunit;

namespace Tanka.GraphQL.Tests.Experimental;

public class QueryFacts
{
    [Fact]
    public async Task Simple_Scalar()
    {
        /* Given */
        var schema = await new GraphQL.Experimental.TypeSystem.SchemaBuilder()
            .Add("""
            type Query 
            {
                version: String!
            }
            """)
            .Build(new GraphQL.Experimental.ResolversMap
            {
                ["Query"] = new()
                {
                    { "version", ctx => new ValueTask<object>("1.0") }
                }
            });

        ExecutableDocument query = """
            {
                version
            }
            """;

        /* When */
        var result = await new GraphQL.Experimental.Executor(schema)
            .ExecuteAsync(new GraphQLRequest(query));

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
        var schema = await new GraphQL.Experimental.TypeSystem.SchemaBuilder()
            .Add("""
            type System 
            {
                version: String!
            }

            type Query 
            {
                system: System!
            }
            """)
            .Build(new GraphQL.Experimental.ResolversMap
            {
                ["Query"] = new()
                {
                    { "system", ctx => new ValueTask<object?>("system") }
                },
                ["System"] = new()
                {
                    { "version", ctx => new ValueTask<object>("1.0") }
                }
            });

        ExecutableDocument query = """
            {
                system {
                    version
                }
            }
            """;

        /* When */
        var result = await new GraphQL.Experimental.Executor(schema)
            .ExecuteAsync(new GraphQLRequest(query));

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