using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests
{
    public class ExecutionPathFacts
    {
        private Task<ISchema> _schema;

        public ExecutionPathFacts()
        {
            // schema
            var builder = new SchemaBuilder();

            builder.Object("Node", out var node)
                .Field(node, "child", node)
                .Field(node, "path", new List(ScalarType.String))
                .Field(node, "value", ScalarType.String)
                .Field(node, "children", new List(node));

            builder.Query(out var query)
                .Field(query, "root", node);

            builder.Mutation(out var mutation)
                .Field(mutation, "root", node);

            var schema = builder.Build();

            var resolvers = new ResolverMap()
            {
                {
                    "Query", new FieldResolverMap()
                    {
                        {"root", context => Task.FromResult(Resolve.As(new {}))}
                    }
                },
                {
                    "Mutation", new FieldResolverMap()
                    {
                        {"root", context => Task.FromResult(Resolve.As(new {}))}
                    }
                },
                {
                    "Node", new FieldResolverMap()
                    {
                        {"child", context => Task.FromResult(Resolve.As(new {}))},
                        {"children", context => Task.FromResult(Resolve.As(new []
                        {
                            new {id = 0},
                            new {id = 1}
                        }))},
                        {"value", context => Task.FromResult(Resolve.As("value"))},
                        {"path", context => Task.FromResult(Resolve.As(context.Path.Segments))}
                    }
                }
            };

            _schema = SchemaTools.MakeExecutableSchemaAsync(schema, resolvers);
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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = await _schema,
                Document =  Parser.ParseDocument(query)
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

            var rootChildrenFirstPath = result.Select("root", "children",0,"path") as IEnumerable<object>;
            Assert.Equal(new object[]
            {
                "root",
                "children",
                "0",
                "path"
            }, rootChildrenFirstPath);
        }

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
            var result = await Executor.ExecuteAsync(new ExecutionOptions()
            {
                Schema = await _schema,
                Document =  Parser.ParseDocument(query)
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

            var rootChildrenFirstPath = result.Select("root", "children",0,"path") as IEnumerable<object>;
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
