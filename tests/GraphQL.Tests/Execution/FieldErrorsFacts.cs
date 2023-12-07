using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

public class Query
{
    public Query()
    {
        Container = new();
        Custom = new();
    }

    public Container Container { get; }

    public CustomContainer Custom { get; }
}

public class Container
{
    public string NonNull => "value";
    public string NonNull_WithNull => null!;

    public List<string> NonNullList_AsNull => null!;

    public List<string> NonNullList_WithNullSecondItem => new()
    {
        "first",
        null!,
        "third"
    };
}

public class CustomContainer
{
}

public class FieldErrorsFacts
{
    private readonly ISchema _schema;

    public FieldErrorsFacts()
    {
        Query = new();
        var builder = new SchemaBuilder()
            .Add(
                @"
                    type Container {
                        nonNullWithNull: String!
                        nonNullListAsNull: [String]!
                        nonNullListWithNonNullItem: [String!]!
                        nonNullListWithNullItem: [String]!
                    }

                    type CustomErrorContainer {
                        nonNullWithCustomError: String!
                        nullableWithCustomError: String
                        nonNullListWithCustomError: [String]!
                        nonNullListItemWithCustomError: [String!]!
                    }

                    type Query {
                        container: Container
                        custom: CustomErrorContainer
                    }

                    schema {
                        query : Query
                    }
                ");

        var resolvers = new ResolversMap
        {
            ["Container"] = new()
            {
                { "nonNullWithNull", context => context.ResolveAsPropertyOf<Container>(c => c.NonNull_WithNull) },
                { "nonNullListAsNull", context => context.ResolveAsPropertyOf<Container>(c => c.NonNullList_AsNull) },
                { "nonNullListWithNonNullItem", context => context.ResolveAsPropertyOf<Container>(c => c.NonNullList_WithNullSecondItem) },
                { "nonNullListWithNullItem", context => context.ResolveAsPropertyOf<Container>(c => c.NonNullList_WithNullSecondItem) }
            },
            ["CustomErrorContainer"] = new()
            {
                { "nonNullWithCustomError", context => throw new InvalidOperationException("error") },
                { "nullableWithCustomError", context => throw new InvalidOperationException("error") },
                { "nonNullListWithCustomError", context => throw new InvalidOperationException("error") },
                { "nonNullListItemWithCustomError", context => throw new InvalidOperationException("error") }
            },
            ["Query"] = new()
            {
                { "container", context => context.ResolveAs(Query.Container) },
                { "custom", context => context.ResolveAs(Query.Custom) }
            }
        };

        _schema = builder.Build(resolvers).Result;
    }

    public Query Query { get; }

    [Fact]
    public async Task NullValue_resolved_for_non_null_field()
    {
        /* Given */
        var query =
            @"
                {
                    container {
                        nonNullWithNull
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"{
  ""data"": {
    ""container"": null
  },
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot complete value for field 'nonNullWithNull: String!'. Completed value would be null for non-null field"",
      ""locations"": [
        {
          ""line"": 4,
          ""column"": 26
        }
      ],
      ""path"": [
        ""container"",
        ""nonNullWithNull""
      ]
    }
  ]
}");
    }

    [Fact]
    public async Task NullValue_resolved_for_non_null_list_field()
    {
        /* Given */
        var query =
            @"
                {
                    container {
                        nonNullListAsNull
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"{
  ""data"": {
    ""container"": null
  },
  ""extensions"": null,
  ""errors"": [
    {
      ""message"": ""Cannot complete value for field 'nonNullListAsNull: [String]!'. Completed value would be null for non-null field"",
      ""locations"": [
        {
          ""line"": 4,
          ""column"": 26
        }
      ],
      ""path"": [
        ""container"",
        ""nonNullListAsNull""
      ]
    }
  ]
}");
    }

    [Fact]
    public async Task NullValue_item_resolved_for_non_null_list_field()
    {
        /* Given */
        var query =
            @"
                {
                    container {
                        nonNullListWithNullItem
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        // this is valid scenario as the requirement is to only have the list non null (items can be null)
        result.ShouldMatchJson(@"{
                  ""data"": {
                    ""container"": {
                      ""nonNullListWithNullItem"": [
                        ""first"",
                        null,
                        ""third""
                      ]
                    }
                  }
                }");
    }

    [Fact]
    public async Task NullValue_item_resolved_for_non_null_list_with_non_null_items_field()
    {
        /* Given */
        var query =
            @"
                {
                    container {
                        nonNullListWithNonNullItem
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"{
  ""data"": {
    ""container"": null
  },
  ""errors"": [
    {
      ""locations"": [
        {
          ""line"": 4,
          ""column"": 26
        }
      ],
      ""message"": ""Cannot complete value for field 'nonNullListWithNonNullItem: [String!]!'. Completed value would be null for non-null field"",
      ""path"": [
        ""container"",
        ""nonNullListWithNonNullItem""
      ]
    }
  ]
}");
    }

    [Fact]
    public async Task Exception_thrown_by_NonNull_field()
    {
        /* Given */
        var query =
            @"
                {
                    custom {
                        nonNullWithCustomError
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"
                {
                  ""data"": {
                    ""custom"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""error"",
                      ""locations"": [
                        {
                          ""line"": 4,
                          ""column"": 26
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nonNullWithCustomError""
                      ]
                    }
                  ]
                }
                ");
    }

    [Fact]
    public async Task Exception_thrown_by_nullable_field()
    {
        /* Given */
        var query =
            @"
                {
                    custom {
                        nullableWithCustomError
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"
                {
                  ""data"": {
                    ""custom"": {
                      ""nullableWithCustomError"": null
                    }
                  },
                  ""errors"": [
                    {
                      ""message"": ""error"",
                      ""locations"": [
                        {
                          ""line"": 4,
                          ""column"": 26
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nullableWithCustomError""
                      ]
                    }
                  ]
                }
                ");
    }

    [Fact]
    public async Task Exception_thrown_by_nonNullList_field()
    {
        /* Given */
        var query =
            @"
                {
                    custom {
                        nonNullListWithCustomError
                    }
                }
                ";


        /* When */
        var result = await Executor.Execute(_schema, query);

        /* Then */
        result.ShouldMatchJson(
            @"
                {
                  ""data"": {
                    ""custom"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""error"",
                      ""locations"": [
                        {
                          ""line"": 4,
                          ""column"": 26
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nonNullListWithCustomError""
                      ]
                    }
                  ]
                }
                ");
    }
}