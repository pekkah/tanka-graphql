using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests
{
    public class FederationFacts
    {
        public FederationFacts()
        {
            Sut = SchemaFactory.Create();
        }

        public ISchema Sut { get; }

        [Fact]
        public async Task Query_representation()
        {
            /* Given */

            /* When */
            var result = await Executor
                .ExecuteAsync(new ExecutionOptions
                {
                    IncludeExceptionDetails = true,
                    Schema = Sut,
                    Document = @"
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
}",
                    VariableValues = new Dictionary<string, object>
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

            /* When */
            var result = await Executor
                .ExecuteAsync(new ExecutionOptions
                {
                    IncludeExceptionDetails = true,
                    Schema = Sut,
                    Document = @"query { _service { sdl } }"
                });

            /* Then */
            result.ShouldMatchJson(@"
{
  ""data"": {
    ""_service"": {
      ""sdl"": ""type Review  @key(fields: \""id\"") { id: ID! body: String author: User @provides(fields: \""username\"") product: Product }  type User  @key(fields: \""id\"") @extends { id: ID! @external username: String @external reviews: [Review] }  type Product  @key(fields: \""upc\"") @extends { upc: String! @external reviews: [Review] }""
    }
  },
  ""extensions"": null,
  ""errors"": null
}");
        }
    }
}