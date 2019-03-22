using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using GraphQLParser.AST;
using tanka.graphql.channels;
using tanka.graphql.sdl;

namespace tanka.graphql.benchmarks
{
    public class BufferedEventChannel : EventChannel<string>
    {
        public override void OnSubscribed(SubscribeResult subscription)
        {
            subscription.WriteAsync("value").AsTask().Wait();
        }
    }

    public static class Utils
    {
        public static ISchema InitializeSchema()
        {
            var events = new BufferedEventChannel();
            var builder = new SchemaBuilder();
            Sdl.Import(Parser.ParseDocument(
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
                    "), builder);

            var resolvers = new ResolverMap
            {
                {
                    "Query", new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Mutation", new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Subscription", new FieldResolverMap()
                    {
                        {
                            "simple", 
                            (context, unsubscribe) => ResolveSync.Subscribe(events, unsubscribe), 
                            context => new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchema(
                builder, 
                resolvers,
                resolvers);

            return schema;
        }

        public static GraphQLDocument InitializeQuery()
        {
            return Parser.ParseDocument(@"
{
    simple
}");
        }

        public static GraphQLDocument InitializeMutation()
        {
            return Parser.ParseDocument(@"
mutation {
    simple
}");
        }

        public static GraphQLDocument InitializeSubscription()
        {
            return Parser.ParseDocument(@"
subscription {
    simple
}");
        }
    }
}