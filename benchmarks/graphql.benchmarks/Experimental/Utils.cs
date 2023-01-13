using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.TypeSystem;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Benchmarks.Experimental;

public static class Utils
{
    public static Task<ISchema> InitializeSchema()
    {
        var events = new SingleValueEventChannel();
        var builder = new Tanka.GraphQL.Experimental.TypeSystem.SchemaBuilder()
            .Add("""
                    type Query {
                        simple: String
                        complex: Complex!
                    }

                    type Mutation {
                        simple: String
                    }

                    type Subscription {
                        simple: String
                    }

                    type Complex {
                        field: Int!
                    }

                    schema {
                        query: Query
                        mutation: Mutation
                        subscription: Subscription
                    }
                    """);

        var resolvers = new Tanka.GraphQL.Experimental.ResolversMap
        {
            {
                "Query", new Tanka.GraphQL.Experimental.FieldResolversMap
                {
                    { "simple", context => new ValueTask<object?>("value") },
                    { "complex", context => new ValueTask<object?>("Complex") }
                }
            },
            {
                "Mutation", new Tanka.GraphQL.Experimental.FieldResolversMap
                {
                    { "simple", context => new ValueTask<object?>("value") }
                }
            },
            {
                "Complex", new Tanka.GraphQL.Experimental.FieldResolversMap()
                {
                    {"field", context => new ValueTask<object>(123)}
                }
            }
            /*{
                "Subscription", new FieldResolversMap
                {
                    {
                        "simple",
                        (context, unsubscribe) => ResolveSync.Subscribe(events, unsubscribe),
                        context => new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue))
                    }
                }
            }*/
        };

        return builder.Build(resolvers, resolvers);
    }

    public static ExecutableDocument InitializeQuery()
    {
        return @"
{
    simple
}";
    }

    public static ExecutableDocument InitializeComplexQuery()
    {
        return """
            {
                complex {
                    field
                }
            }
            """;
    }

    public static ExecutableDocument InitializeMutation()
    {
        return @"
mutation {
    simple
}";
    }

    public static ExecutableDocument InitializeSubscription()
    {
        return @"
subscription {
    simple
}";
    }
}