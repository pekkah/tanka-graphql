using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
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
        TypeSystemDocument schemaOne =
                @"
                    extend type User {
                        id: ID!
                        name: String!
                    }

                    extend type Query {
                        userById(id: ID!): User
                    }

                    extend schema {
                        query: Query
                    }
                    ";

        TypeSystemDocument schemaTwo = 
                @"
                    type Address {
                        city: String!
                    }

                    type User {
                        address: Address!
                    }

                    type Query {

                    }
                    ";

        var schemaOneResolvers = RemoteSchemaTools.CreateLinkResolvers(
            schemaOne,
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

        var schemaTwoResolvers = 
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
            };

        var resolvers = schemaOneResolvers + schemaTwoResolvers;
        var schema = await new SchemaBuilder()
            .Add(schemaOne)
            .Add(schemaTwo)
            .Build(resolvers);

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