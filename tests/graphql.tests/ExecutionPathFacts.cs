using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.Tools;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests
{
    public class ExecutionPathFacts
    {
        public ExecutionPathFacts()
        {
            // schema
            var builder = new SchemaBuilder();

            builder.Object("Node", out var node)
                .Connections(connect => connect
                    .Field(node, "child", node)
                    .Field(node, "path", new List(ScalarType.String))
                    .Field(node, "value", ScalarType.String)
                    .Field(node, "children", new List(node)));

            builder.Query(out var query)
                .Connections(connect => connect
                    .Field(query, "root", node));

            builder.Mutation(out var mutation)
                .Connections(connect => connect
                    .Field(mutation, "root", node));

            var schema = builder.Build();

            var resolvers = new ObjectTypeMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {"root", context => new ValueTask<IResolverResult>(Resolve.As(new { }))}
                    }
                },
                {
                    "Mutation", new FieldResolversMap
                    {
                        {"root", context => new ValueTask<IResolverResult>(Resolve.As(new { }))}
                    }
                },
                {
                    "Node", new FieldResolversMap
                    {
                        {"child", context => new ValueTask<IResolverResult>(Resolve.As(new { }))},
                        {
                            "children", context => new ValueTask<IResolverResult>(Resolve.As(new[]
                            {
                                new {id = 0},
                                new {id = 1}
                            }))
                        },
                        {"value", context => new ValueTask<IResolverResult>(Resolve.As("value"))},
                        {"path", context => new ValueTask<IResolverResult>(Resolve.As(context.Path.Segments))}
                    }
                }
            };

            _schema = SchemaTools.MakeExecutableSchema(schema, resolvers);
        }

        private readonly ISchema _schema;

        [Fact]
        public async Task Mutation_path_should_match()
        {
            /* Given */
            var query = @"
mutation Root {
    root {
        value
        child {
            value
            path
        }
        children {
            value
            path
        }
        path
    }
}
";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = Parser.ParseDocument(query)
            });

            /* Then */
            var rootPath = result.Select("root", "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "path"
            }, rootPath);
            var rootChildPath = result.Select("root", "child", "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "child",
                "path"
            }, rootChildPath);

            var rootChildrenFirstPath = result.Select("root", "children", 0, "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "children",
                "0",
                "path"
            }, rootChildrenFirstPath);
        }

        [Fact]
        public async Task Query_path_should_match()
        {
            /* Given */
            var query = @"
{
    root {
        value
        child {
            value
            path
        }
        children {
            value
            path
        }
        path
    }
}
";

            /* When */
            var result = await Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = Parser.ParseDocument(query)
            });

            /* Then */
            var rootPath = result.Select("root", "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "path"
            }, rootPath);
            var rootChildPath = result.Select("root", "child", "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "child",
                "path"
            }, rootChildPath);

            var rootChildrenFirstPath = result.Select("root", "children", 0, "path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "children",
                "0",
                "path"
            }, rootChildrenFirstPath);
        }
    }
}