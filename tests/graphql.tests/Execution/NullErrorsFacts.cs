using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

public class NullErrorsFacts
{
    private readonly ISchema _schema;

    public NullErrorsFacts()
    {
        var builder = new SchemaBuilder()
            .Add(@"
    type Nest 
    {
        nestedNonNull: String!
    }

    type Query 
    {
        nonNull: String!
        nonNullNested: Nest!
        nonNullListItem: [String!]
        nonNullList: [String]!
        nullableNested: Nest
        nullable: String
    }
    ");


        var nestedNonNullData = new Dictionary<string, string>
        {
            ["nestedNonNull"] = null
        };

        IResolverMap resolvers = new ResolversMap
        {
            ["Query"] = new()
            {
                { "nonNull", context => new ValueTask<IResolverResult>(Resolve.As(null)) },
                { "nonNullNested", context => new ValueTask<IResolverResult>(Resolve.As(nestedNonNullData)) },
                {
                    "nonNullListItem",
                    context => new ValueTask<IResolverResult>(Resolve.As(new[] { "str", null, "str" }))
                },
                { "nonNullList", context => new ValueTask<IResolverResult>(Resolve.As(null)) },
                { "nullableNested", context => new ValueTask<IResolverResult>(Resolve.As(nestedNonNullData)) },
                { "nullable", context => new ValueTask<IResolverResult>(Resolve.As("hello")) }
            },

            ["Nest"] = new()
            {
                { "nestedNonNull", Resolve.PropertyOf<Dictionary<string, string>>(d => d["nestedNonNull"]) }
            }
        };

        _schema = builder.Build(resolvers).Result;
    }


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
            Document = query
        }).ConfigureAwait(false);


        /* Then */
        result.ShouldMatchJson(@"
                {
  ""data"": null,
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
      ""locations"": [
        {
          ""line"": 4,
          ""column"": 9
        }
      ],
      ""path"": [
        ""nonNullNested"",
        ""nestedNonNull""
      ],
      ""extensions"": {
        ""code"": ""NULLVALUEFORNONNULLTYPE""
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
            Document = query
        }).ConfigureAwait(false);


        /* Then */
        result.ShouldMatchJson(@"
                {
  ""data"": {
    ""nullable"": ""hello"",
    ""nullableNested"": null
  },
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
      ""locations"": [
        {
          ""line"": 5,
          ""column"": 10
        }
      ],
      ""path"": [
        ""nullableNested"",
        ""nestedNonNull""
      ],
      ""extensions"": {
        ""code"": ""NULLVALUEFORNONNULLTYPE""
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
            Document = query
        }).ConfigureAwait(false);


        /* Then */
        result.ShouldMatchJson(@"
                {
  ""data"": {
    ""nullableNested"": null
  },
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot return null for non-nullable field 'Nest.nestedNonNull'."",
      ""locations"": [
        {
          ""line"": 4,
          ""column"": 9
        }
      ],
      ""path"": [
        ""nullableNested"",
        ""nestedNonNull""
      ],
      ""extensions"": {
        ""code"": ""NULLVALUEFORNONNULLTYPE""
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
            Document = query
        }).ConfigureAwait(false);


        /* Then */
        result.ShouldMatchJson(@"
                {
  ""data"": null,
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot return null for non-nullable field 'Query.nonNull'."",
      ""locations"": [
        {
          ""line"": 3,
          ""column"": 6
        }
      ],
      ""path"": [
        ""nonNull""
      ],
      ""extensions"": {
        ""code"": ""NULLVALUEFORNONNULLTYPE""
      }
    }
  ]
}");
    }
}