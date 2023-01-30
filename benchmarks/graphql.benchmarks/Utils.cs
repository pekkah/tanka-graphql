using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Benchmarks;

public static class Utils
{
    public static Task<ISchema> InitializeSchema()
    {
        //var events = new SingleValueEventChannel();
        var builder = new SchemaBuilder()
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

        var resolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { "simple", context => context.ResolveAs("value") },
                    { "complex", context => context.ResolveAs("Complex") }
                }
            },
            {
                "Mutation", new FieldResolversMap
                {
                    { "simple", context => context.ResolveAs("value") }
                }
            },
            {
                "Complex", new FieldResolversMap
                {
                    { "field", context => context.ResolveAs(123) }
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