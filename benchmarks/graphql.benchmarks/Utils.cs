using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Benchmarks;

public static class Utils
{
    public static Task<ISchema> InitializeSchema()
    {
        var events = new SingleValueEventChannel();
        var builder = new SchemaBuilder()
            .Add(Parser.ParseTypeSystemDocument(
                @"
                    type Query {
                        simple: String
                    }

                    type Mutation {
                        simple: String
                    }

                    type Subscription {
                        simple: String
                    }

                    schema {
                        query: Query
                        mutation: Mutation
                        subscription: Subscription
                    }
                    "));

        var resolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { "simple", context => new ValueTask<IResolverResult>(Resolve.As("value")) }
                }
            },
            {
                "Mutation", new FieldResolversMap
                {
                    { "simple", context => new ValueTask<IResolverResult>(Resolve.As("value")) }
                }
            },
            {
                "Subscription", new FieldResolversMap
                {
                    {
                        "simple",
                        (context, unsubscribe) => ResolveSync.Subscribe(events, unsubscribe),
                        context => new ValueTask<IResolverResult>(Resolve.As(context.ObjectValue))
                    }
                }
            }
        };

        return builder.Build(resolvers, resolvers);
    }

    public static ExecutableDocument InitializeQuery()
    {
        return Parser.ParseDocument(@"
{
    simple
}");
    }

    public static ExecutableDocument InitializeMutation()
    {
        return Parser.ParseDocument(@"
mutation {
    simple
}");
    }

    public static ExecutableDocument InitializeSubscription()
    {
        return Parser.ParseDocument(@"
subscription {
    simple
}");
    }
}