using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.Tests.Data;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution
{
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
        public string NonNull_WithNull => null;

        public string NonNull => "value";

        public List<string> NonNullList_AsNull => null;

        public List<string> NonNullList_WithNullSecondItem => new List<string>()
        {
            "first",
            null,
            "third"
        };
    }

    public class CustomContainer
    {

    }

    public class FieldErrorsFacts
    {
        private readonly ISchema _schema;

        public Query Query { get; }

        public FieldErrorsFacts()
        {
            Query = new Query();
            var builder = new SchemaBuilder()
                .Sdl(Parser.ParseDocument(
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
                "));

            var resolvers = new ObjectTypeMap()
            {
                ["Container"] = new FieldResolversMap()
                {
                    {"nonNullWithNull", Resolve.PropertyOf<Container>(c => c.NonNull_WithNull)},
                    {"nonNullListAsNull", Resolve.PropertyOf<Container>(c => c.NonNullList_AsNull)},
                    {"nonNullListWithNonNullItem", Resolve.PropertyOf<Container>(c => c.NonNullList_WithNullSecondItem)},
                    {"nonNullListWithNullItem", Resolve.PropertyOf<Container>(c => c.NonNullList_WithNullSecondItem)}
                },
                ["CustomErrorContainer"] = new FieldResolversMap()
                {
                    {"nonNullWithCustomError", context => throw new InvalidOperationException("error")},
                    {"nullableWithCustomError", context => throw new InvalidOperationException("error")},
                    {"nonNullListWithCustomError", context => throw new InvalidOperationException("error")},
                    {"nonNullListItemWithCustomError", context => throw new InvalidOperationException("error")}
                },
                ["Query"] = new FieldResolversMap()
                {
                    {"container" , context => new ValueTask<IResolveResult>(Resolve.As(Query.Container))},
                    {"custom", context=> new ValueTask<IResolveResult>(Resolve.As(Query.Custom))}
                }
            };

            _schema = SchemaTools.MakeExecutableSchema(
                builder,
                resolvers);
        }

        [Fact]
        public async Task NullValue_resolved_for_non_null_field()
        {
            /* Given */
            var query = Parser.ParseDocument(
                @"
                {
                    container {
                        nonNullWithNull
                    }
                }
                ");


            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                      ""message"": ""Cannot return null for non-nullable field 'Container.nonNullWithNull'."",
                      ""locations"": [
                        {
                          ""end"": 112,
                          ""start"": 75
                        }
                      ],
                      ""path"": [
                        ""container"",
                        ""nonNullWithNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                      ""message"": ""Cannot return null for non-nullable field 'Container.nonNullListAsNull'."",
                      ""locations"": [
                        {
                          ""end"": 114,
                          ""start"": 75
                        }
                      ],
                      ""path"": [
                        ""container"",
                        ""nonNullListAsNull""
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                      ""message"": ""Cannot return null for non-nullable field 'Container.nonNullListWithNonNullItem'."",
                      ""locations"": [
                        {
                          ""end"": 123,
                          ""start"": 75
                        }
                      ],
                      ""path"": [
                        ""container"",
                        ""nonNullListWithNonNullItem"",
                        1
                      ],
                      ""extensions"": {
                        ""code"": ""NULLVALUEFORNONNULL""
                      }
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                          ""end"": 116,
                          ""start"": 72
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nonNullWithCustomError""
                      ],
                      ""extensions"": {
                        ""code"": ""INVALIDOPERATION""
                      }
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                          ""end"": 117,
                          ""start"": 72
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nullableWithCustomError""
                      ],
                      ""extensions"": {
                        ""code"": ""INVALIDOPERATION""
                      }
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
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
                          ""end"": 120,
                          ""start"": 72
                        }
                      ],
                      ""path"": [
                        ""custom"",
                        ""nonNullListWithCustomError""
                      ],
                      ""extensions"": {
                        ""code"": ""INVALIDOPERATION""
                      }
                    }
                  ]
                }
                ");
        }
    }
}