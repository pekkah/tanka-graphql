﻿using System.Threading.Tasks;
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
}