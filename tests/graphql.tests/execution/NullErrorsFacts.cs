using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution
{
    public class NullErrorsFacts
    {
        public NullErrorsFacts()
        {
            var builder = new SchemaBuilder();
            builder.Object("Nest", out var nested)
                .Connections(connect => connect
                .Field(nested, "nestedNonNull", ScalarType.NonNullString));

            builder.Query(out var query)
                .Connections(connect => connect
                .Field(query, "nonNull", ScalarType.NonNullString)
                .Field(query, "nonNullNested", new NonNull(nested))
                .Field(query, "nonNullListItem", new List(ScalarType.NonNullString))
                .Field(query, "nonNullList", new NonNull(new List(ScalarType.String)))
                .Field(query, "nullableNested", nested)
                .Field(query, "nullable", ScalarType.String));


            var nestedNonNullData = new Dictionary<string, string>
            {
                ["nestedNonNull"] = null
            };

            IResolverMap resolvers = new ObjectTypeMap
            {
                ["Query"] = new FieldResolversMap
                {
                    {"nonNull", context => new ValueTask<IResolverResult>(Resolve.As(null))},
                    {"nonNullNested", context => new ValueTask<IResolverResult>(Resolve.As(nestedNonNullData))},
                    {"nonNullListItem", context => new ValueTask<IResolverResult>(Resolve.As(new[] {"str", null, "str"}))},
                    {"nonNullList", context => new ValueTask<IResolverResult>(Resolve.As(null))},
                    {"nullableNested", context => new ValueTask<IResolverResult>(Resolve.As(nestedNonNullData))},
                    {"nullable", context => new ValueTask<IResolverResult>(Resolve.As("hello"))}
                },

                ["Nest"] = new FieldResolversMap
                {
                    {"nestedNonNull", Resolve.PropertyOf<Dictionary<string, string>>(d => d["nestedNonNull"])}
                }
            };

            _schema = SchemaTools.MakeExecutableSchema(
                builder,
                resolvers);
        }

        private readonly ISchema _schema;


        [Fact]
        public async Task Nested_NonNull_should_produce_error_and_set_graph_to_null()
        {
            /* Given */
            var query = @"
{
    nonNullNested {
       nestedNonNull 
    }
}";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document =  Parser.ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": null,
                  ""errors"": [
                    {
                      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
                      ""locations"": [
                        {
                          ""end"": 50,
                          ""start"": 30
                        }
                      ],
                      ""path"": [
                        ""nonNullNested"",
                        ""nestedNonNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
                    }
                  ]
                }");
        }

        [Fact]
        public async Task Nested_Nullable_NonNull_and_nullable_should_produce_error()
        {
            /* Given */
            var query = @"
{
    nullable
    nullableNested {
        nestedNonNull
    }
}";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document =  Parser.ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": {
                    ""nullableNested"": null,
                    ""nullable"": ""hello""
                  },
                  ""errors"": [
                    {
                      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
                      ""locations"": [
                        {
                          ""end"": 64,
                          ""start"": 45
                        }
                      ],
                      ""path"": [
                        ""nullableNested"",
                        ""nestedNonNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
                    }
                  ]
                }");
        }

        [Fact]
        public async Task Nested_Nullable_NonNull_should_produce_error()
        {
            /* Given */
            var query = @"
{
    nullableNested {
       nestedNonNull 
    }
}";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document =  Parser.ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": {
                    ""nullableNested"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
                      ""locations"": [
                        {
                          ""end"": 51,
                          ""start"": 31
                        }
                      ],
                      ""path"": [
                        ""nullableNested"",
                        ""nestedNonNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
                    }
                  ]
                }");
        }

        [Fact]
        public async Task NonNull_should_produce_error()
        {
            /* Given */
            var query = @"
{
    nonNull
}";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document =  Parser.ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
                {
                  ""data"": null,
                  ""errors"": [
                    {
                      ""message"": ""Cannot return null for non-nullable field 'Query.nonNull'."",
                      ""locations"": [
                        {
                          ""end"": 16,
                          ""start"": 7
                        }
                      ],
                      ""path"": [
                        ""nonNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
                    }
                  ]
                }");
        }
    }
}