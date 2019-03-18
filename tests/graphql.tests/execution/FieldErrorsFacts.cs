using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.sdl;
using tanka.graphql.tests.data;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.execution
{
    public class Query
    {
        public Query()
        {
            Container = new Container();
        }

        public Container Container { get; }
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

    public class FieldErrorsFacts
    {
        private ISchema _schema;

        public Query Query { get; }

        public FieldErrorsFacts()
        {
            Query = new Query();
            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(
                @"
                    type Container {
                        nonNullWithNull: String!
                        nonNullListAsNull: [String]!
                        nonNullListWithNonNullItem: [String!]!
                        nonNullListWithNullItem: [String]!
                    }

                    type Query {
                        container: Container
                    }

                    schema {
                        query : Query
                    }
                "), builder);

            var resolvers = new ResolverMap()
            {
                ["Container"] = new FieldResolverMap()
                {
                    {"nonNullWithNull", Resolve.PropertyOf<Container>(c => c.NonNull_WithNull)},
                    {"nonNullListAsNull", Resolve.PropertyOf<Container>(c => c.NonNullList_AsNull)},
                    {"nonNullListWithNonNullItem", Resolve.PropertyOf<Container>(c => c.NonNullList_WithNullSecondItem)},
                    {"nonNullListWithNullItem", Resolve.PropertyOf<Container>(c => c.NonNullList_WithNullSecondItem)}
                },
                ["Query"] = new FieldResolverMap()
                {
                    {"container" , context => new ValueTask<IResolveResult>(Resolve.As(Query.Container))}
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
            result.ShouldMatchJson(@"{
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = _schema,
                Document = query
            });

            /* Then */
            result.ShouldMatchJson(@"{
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
            result.ShouldMatchJson(@"{
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
                  ]
                }
              ]
            }");
        }
    }
}