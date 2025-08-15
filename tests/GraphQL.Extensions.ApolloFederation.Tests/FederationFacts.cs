using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class FederationFacts
{
    public FederationFacts()
    {
        Sut = SchemaFactory.Create().Result;
    }

    public ISchema Sut { get; }

    [Fact]
    public async Task Query_representation()
    {
        /* Given */

        /* When */
        var result = await new Executor(Sut)
            .Execute(new GraphQLRequest
            {
                Query = """
                    query($representations:[_Any!]!) {
                        _entities(representations:$representations) {
                            ...on User {
                                reviews { 
                                    id 
                                    body 
                                    author {
                                        username
                                    }
                                    product {
                                        upc
                                    }
                                }
                            }
                        }
                    }
                    """,
                Variables = new Dictionary<string, object>
                {
                    ["representations"] = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "User",
                            ["id"] = 1
                        }
                    }
                }
            });

        /* Then */
        result.ShouldMatchJson(@"
{
  ""data"": {
    ""_entities"": [
      {
        ""reviews"": [
          {
            ""id"": ""1"",
            ""body"": ""Love it!"",
            ""author"": {
              ""username"": ""@ada""
            },
            ""product"": {
              ""upc"": ""1""
            }
          },
          {
            ""id"": ""2"",
            ""body"": ""Too expensive!"",
            ""author"": {
              ""username"": ""@ada""
            },
            ""product"": {
              ""upc"": ""2""
            }
          }
        ]
      }
    ]
  },
  ""extensions"": null,
  ""errors"": null
}");
    }

    [Fact]
    public async Task Query_sdl()
    {
        /* Given */

        // Debug: Check if _service field exists in schema
        var queryType = Sut.GetNamedType("Query") as ObjectDefinition;
        var serviceField = queryType?.Fields?.FirstOrDefault(f => f.Name == "_service");

        /* When */
        var result = await new Executor(Sut)
            .Execute(new GraphQLRequest
            {
                Query = @"query { _service { sdl } }"
            });

        /* Then */
        Assert.Null(result.Errors);
        result.ShouldMatchJson("""
            {
              "data": {
                "_service": {
                  "sdl": "type Product  @key(fields: \"upc\") @extends { upc: String! @external reviews: [Review] }  type Review  @key(fields: \"id\") { id: ID! body: String author: User @provides(fields: \"username\") product: Product }  type User  @key(fields: \"id\") @extends { id: ID! @external username: String @external reviews: [Review] }"
                }
              },
              "extensions": null,
              "errors": null
            }
            """);
    }
}