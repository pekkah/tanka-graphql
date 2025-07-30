using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class ExecutionPathFacts
{
    private readonly ISchema _schema;

    public ExecutionPathFacts()
    {
        // schema
        var builder = new SchemaBuilder();

        builder.Add((TypeSystemDocument)@"
type Node {
    child: Node
    path: [String]
    value: String
    children: [Node]
}

type Query {
    root: Node
}

type Mutation {
    root: Node
}

");

        var resolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { "root", context => context.ResolveAs(new { }) }
                }
            },
            {
                "Mutation", new FieldResolversMap
                {
                    { "root", context => context.ResolveAs(new { }) }
                }
            },
            {
                "Node", new FieldResolversMap
                {
                    { "child", context => context.ResolveAs(new { }) },
                    {
                        "children", context => context.ResolveAs(new[]
                        {
                            new { id = 0 },
                            new { id = 1 }
                        })
                    },
                    { "value", context => context.ResolveAs("value") },
                    { "path", context => context.ResolveAs(context.Path.Segments.ToArray()) }
                }
            }
        };

        _schema = builder.Build(resolvers).Result;
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
        var result = await Executor.Execute(_schema, query);

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
        var result = await Executor.Execute(_schema, query);

        /* Then */
        var rootPath = result.Select("root", "path") as IEnumerable<object>;
        Assert.Equal(new List<object>()
        {
            "root",
            "path"
        }, rootPath);
        var rootChildPath = result.Select("root", "child", "path") as IEnumerable<object>;
        Assert.Equal(new List<object>()
        {
            "root",
            "child",
            "path"
        }, rootChildPath);

        var rootChildrenFirstPath = result.Select("root", "children", 0, "path") as IEnumerable<object>;
        Assert.Equal(new List<object>()
        {
            "root",
            "children",
            "0",
            "path"
        }, rootChildrenFirstPath);
    }
}