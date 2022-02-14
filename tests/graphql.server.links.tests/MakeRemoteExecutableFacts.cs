using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Server.Links.Tests;

public class MakeRemoteExecutableFacts
{
    [Fact]
    public async Task Execute_with_StaticLink()
    {
        /* Given */
        var schemaOneBuilder = new SchemaBuilder()
            .Add(
                @"
                    type User {
                        id: ID!
                        name: String!
                    }

                    type Query {
                        userById(id: ID!): User
                    }

                    schema {
                        query: Query
                    }
                    ");

        var schemaTwoBuilder = new SchemaBuilder()
            .Add(
                @"
                    type Address {
                        city: String!
                    }

                    type User {
                        address: Address!
                    }

                    type Query {

                    }
                    "
            );

        var schemaOne = RemoteSchemaTools.MakeRemoteExecutable(
            schemaOneBuilder,
            RemoteLinks.Static(new ExecutionResult
            {
                Data = new Dictionary<string, object>
                {
                    ["userById"] = new Dictionary<string, object>
                    {
                        ["id"] = "1",
                        ["name"] = "name"
                    }
                }
            }));

        var schemaTwo = await schemaTwoBuilder.Build(
            new ResolversMap
            {
                ["Address"] = new()
                {
                    { "city", context => ResolveSync.As(context.ObjectValue) }
                },
                ["User"] = new()
                {
                    { "address", context => ResolveSync.As("Vantaa") }
                }
            });

        var schema = await new SchemaBuilder()
            //.Merge(schemaOne, schemaTwo)
            .Build(new SchemaBuildOptions());

        /* When */
        var result = await Executor.ExecuteAsync(new ExecutionOptions
        {
            Schema = schema,
            Document = @"
                {
                    userById(id: ""1"") {
                        id
                        name
                        address {
                            city
                        }
                    }
                }"
        });

        /* Then */
        result.ShouldMatchJson(
            @"
                {
                  ""data"": {
                    ""userById"": {
                      ""address"": {
                        ""city"": ""Vantaa""
                      },
                      ""name"": ""name"",
                      ""id"": ""1""
                    }
                  }
                }
                ");
    }
}