using System.Threading.Tasks;
using tanka.graphql.introspection;
using tanka.graphql.tests.data;
using tanka.graphql.type;
using Xunit;
using static tanka.graphql.Parser;

// ReSharper disable InconsistentNaming

namespace tanka.graphql.tests.introspection
{
    public class IntrospectSchemaFacts
    {
        public IntrospectSchemaFacts()
        {
            var type1 = new ObjectType(
                ObjectTypeName,
                new Fields
                {
                    {ScalarFieldName, ScalarType.Int}
                });

            var query = new ObjectType(
                "Query",
                new Fields
                {
                    {"object", type1}
                });

            var mutation = new ObjectType(
                "Mutation",
                new Fields());

            var subscription = new ObjectType(
                "Subscription",
                new Fields());

            _sourceSchema = new Schema(query, mutation, subscription);
            _introspectionSchema = Introspect.SchemaAsync(_sourceSchema)
                .GetAwaiter()
                .GetResult();
        }

        private readonly Schema _sourceSchema;
        private readonly ISchema _introspectionSchema;

        public const string ObjectTypeName = "Object";
        public const string ScalarFieldName = "int";

        private async Task<ExecutionResult> QueryAsync(string query)
        {
            return await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _introspectionSchema,
                Document = ParseDocument(query)
            });
        }

        [Fact]
        public async Task Schema_root_types()
        {
            /* Given */
            var query = @"{ 
                        __schema {
                                queryType { name }
                                mutationType { name }
                                subscriptionType { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""queryType"": {
                        ""name"": ""Query""
                      },
                      ""mutationType"": {
                        ""name"": ""Mutation""
                      },
                      ""subscriptionType"": {
                        ""name"": ""Subscription""
                      }
                    }
                  }
                }");
        }

        [Fact]
        public async Task Schema_types()
        {
            /* Given */
            var query = @"{ 
                        __schema {
                                types { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""types"": [
                        {
                          ""name"": ""Int""
                        },
                        {
                          ""name"": ""Object""
                        },
                        {
                          ""name"": ""Query""
                        },
                        {
                          ""name"": ""Mutation""
                        },
                        {
                          ""name"": ""Subscription""
                        }
                      ]
                    }
                  }
                }");
        }

        [Fact]
        public async Task Schema_directives()
        {
            /* Given */
            var query = @"{ 
                        __schema {
                                directives { name }
                            }
                        }";

            /* When */
            var result = await QueryAsync(query);

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""__schema"": {
                      ""directives"": [
                        {
                          ""name"": ""include""
                        },
                        {
                          ""name"": ""skip""
                        }
                      ]
                    }
                  }
                }");
        }
    }
}