using System.Collections.Generic;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.type;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using Xunit;
using static tanka.graphql.Executor;
using static tanka.graphql.Parser;
using static tanka.graphql.resolvers.Resolve;
using static tanka.graphql.type.ScalarType;

namespace tanka.graphql.tests
{
    public class ErrorsFacts
    {
        public ErrorsFacts()
        {
            var builder = new SchemaBuilder();
            builder.Object("Nest", out var nested)
                .Connections(connect => connect
                .Field(nested, "nestedNonNull", NonNullString));

            builder.Query(out var query)
                .Connections(connect => connect
                .Field(query, "nonNull", NonNullString)
                .Field(query, "nonNullNested", new NonNull(nested))
                .Field(query, "nonNullListItem", new List(NonNullString))
                .Field(query, "nonNullList", new NonNull(new List(String)))
                .Field(query, "nullableNested", nested)
                .Field(query, "nullable", String));


            var nestedNonNullData = new Dictionary<string, string>
            {
                ["nestedNonNull"] = null
            };

            _resolvers = new ResolverMap
            {
                ["Query"] = new FieldResolverMap
                {
                    {"nonNull", context => new ValueTask<IResolveResult>(As(null))},
                    {"nonNullNested", context => new ValueTask<IResolveResult>(As(nestedNonNullData))},
                    {"nonNullListItem", context => new ValueTask<IResolveResult>(As(new[] {"str", null, "str"}))},
                    {"nonNullList", context => new ValueTask<IResolveResult>(As(null))},
                    {"nullableNested", context => new ValueTask<IResolveResult>(As(nestedNonNullData))},
                    {"nullable", context => new ValueTask<IResolveResult>(As("hello"))}
                },

                ["Nest"] = new FieldResolverMap
                {
                    {"nestedNonNull", PropertyOf<Dictionary<string, string>>(d => d["nestedNonNull"])}
                }
            };

            _schema = builder.Build();
            _executable = SchemaTools.MakeExecutableSchemaAsync(
                _schema,
                _resolvers).Result;
        }

        private readonly ISchema _schema;
        private readonly IResolverMap _resolvers;
        private readonly ISchema _executable;


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
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Schema = _executable,
                Document =  ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
{
  ""errors"": [
     {
      ""message"": ""Field 'Nest.nestedNonNull:String!' is non-null field and cannot be resolved as null. Cannot complete value on non-null field 'nestedNonNull:String!'. Completed value is null."",
      ""locations"": [
        {
          ""end"": 50,
          ""start"": 30
        }
      ],
      ""path"": [
        ""nonNullNested"",
        ""nestedNonNull""
      ]
    },
    {
      ""message"": ""Field 'Query.nonNullNested:Nest!' is non-null field and cannot be resolved as null. Field 'Nest.nestedNonNull:String!' is non-null field and cannot be resolved as null."",
      ""locations"": [
        {
          ""end"": 52,
          ""start"": 7
        }
      ],
      ""path"": [
        ""nonNullNested""
      ]
    }
  ],
  ""data"": null
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
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Schema = _executable,
                Document =  ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
{
  ""errors"": [
    {
      ""message"": ""Field 'Nest.nestedNonNull:String!' is non-null field and cannot be resolved as null. Cannot complete value on non-null field 'nestedNonNull:String!'. Completed value is null."",
      ""locations"": [
        {
          ""end"": 64,
          ""start"": 45
        }
      ],
      ""path"": [
        ""nullableNested"",
        ""nestedNonNull""
      ]
    }
  ],
  ""data"": {
    ""nullable"": ""hello"",
    ""nullableNested"": null
  }
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
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Schema = _executable,
                Document =  ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
{
  ""errors"": [
      {
      ""message"": ""Field 'Nest.nestedNonNull:String!' is non-null field and cannot be resolved as null. Cannot complete value on non-null field 'nestedNonNull:String!'. Completed value is null."",
      ""locations"": [
        {
          ""end"": 51,
          ""start"": 31
        }
      ],
      ""path"": [
        ""nullableNested"",
        ""nestedNonNull""
      ]
    }
  ],
  ""data"": {
    ""nullableNested"": null
  }
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
            var result = await ExecuteAsync(new ExecutionOptions
            {
                Schema = _executable,
                Document =  ParseDocument(query)
            }).ConfigureAwait(false);


            /* Then */
            result.ShouldMatchJson(@"
{
  ""errors"": [
    {
      ""message"": ""Field 'Query.nonNull:String!' is non-null field and cannot be resolved as null. Cannot complete value on non-null field 'nonNull:String!'. Completed value is null."",
      ""locations"": [
        {
          ""end"": 16,
          ""start"": 7
        }
      ],
      ""path"": [
        ""nonNull""
      ]
    }
  ],
  ""data"": null
}");
        }
    }
}