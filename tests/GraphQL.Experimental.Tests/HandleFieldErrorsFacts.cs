using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Experimental.ValueSerialization;
using Tanka.GraphQL.Language.Nodes;
using Xunit;
using static Tanka.GraphQL.Experimental.Core.OperationCoreBuilder;
using static Tanka.GraphQL.Experimental.Request;

namespace Tanka.GraphQL.Experimental.Tests
{
    public class HandleFieldErrorsFacts
    {
        public HandleFieldErrorsFacts()
        {
            Schema = @"
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
                    }";

            Query = new Query();
            Resolvers = new ResolverRoutes()
            {
                {"Container.nonNullWithNull", Resolve<Container>.As(c => c.NonNull_WithNull)},
                {"Container.nonNullListAsNull", Resolve<Container>.As(c => c.NonNullList_AsNull)},
                {"Container.nonNullListWithNonNullItem", Resolve<Container>.As(c => c.NonNullList_WithNullSecondItem)},
                {"Container.nonNullListWithNullItem", Resolve<Container>.As(c => c.NonNullList_WithNullSecondItem)},

                {"CustomErrorContainer.nonNullWithCustomError", Resolve.As(c => throw new InvalidOperationException("error"))},
                {"CustomErrorContainer.nullableWithCustomError", Resolve.As(c => throw new InvalidOperationException("error"))},
                {"CustomErrorContainer.nonNullListWithCustomError", Resolve.As(c => throw new InvalidOperationException("error"))},
                {"CustomErrorContainer.nonNullListItemWithCustomError", Resolve.As(c => throw new InvalidOperationException("error"))},

                {"Query.container" , Resolve.As(_ => Query.Container)},
                {"Query.custom", Resolve.As(_ => Query.Custom)}
            };


            var coerceValue = BuildCoerceValue(new Dictionary<string, CoerceValue>()
            {
                ["String"] = (schema, value, type) => new StringConverter().ParseValue(value)

            });
            ExecuteRequest = UseExecuteRequestSingle(
                Schema,
                (context, options, token) => Task.CompletedTask,
                Resolvers.Resolve,
                default,
                coerceValue,
                (schema, definition, value) => new ValueTask<object?>(value)
            );
        }

        public ExecuteRequestSingle ExecuteRequest { get; }

        protected Query Query { get; }

        protected ExecutableSchema Schema { get; }

        protected ResolverRoutes Resolvers { get; }

        [Fact]
        public async Task NullValue_resolved_for_non_null_field()
        {
            /* Given */
            ExecutableDocument query =
                @"
                {
                    container {
                        nonNullWithNull
                    }
                }
                ";


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""container"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""Field value cannot be null. Field is non-null type."",
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
            var query = Parser.ParseDocument(
                @"
                {
                    container {
                        nonNullListAsNull
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

            /* Then */
            result.ShouldMatchJson(
                @"{
                  ""data"": {
                    ""container"": null
                  },
                  ""errors"": [
                    {
                      ""message"": ""Field value cannot be null. Field is non-null type."",
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
            var query = Parser.ParseDocument(
                @"
                {
                    container {
                        nonNullListWithNullItem
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

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
            var query = Parser.ParseDocument(
                @"
                {
                    container {
                        nonNullListWithNonNullItem
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

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
      ""path"": [
        ""container"",
        ""nonNullListWithNonNullItem""
      ],
      ""message"": ""Field value cannot be null. Field is non-null type.""
    }
  ]
}");
        }

        [Fact]
        public async Task Exception_thrown_by_NonNull_field()
        {
            /* Given */
            var query = Parser.ParseDocument(
                @"
                {
                    custom {
                        nonNullWithCustomError
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

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
            var query = Parser.ParseDocument(
                @"
                {
                    custom {
                        nullableWithCustomError
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

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
            var query = Parser.ParseDocument(
                @"
                {
                    custom {
                        nonNullListWithCustomError
                    }
                }
                ");


            /* When */
            var result = await ExecuteRequest(new RequestOptions()
            {
                Document = query
            });

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

    public class Query
    {
        public Query()
        {
            Container = new Container();
            Custom = new CustomContainer();
        }

        public Container Container { get; }

        public CustomContainer Custom { get; }
    }

    public class Container
    {
        public string? NonNull_WithNull => null;

        public string NonNull => "value";

        public List<string>? NonNullList_AsNull => null;

        public List<string?> NonNullList_WithNullSecondItem => new()
        {
            "first",
            null,
            "third"
        };
    }

    public class CustomContainer
    {
    }
}