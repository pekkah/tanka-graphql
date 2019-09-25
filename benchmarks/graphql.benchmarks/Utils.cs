using System.Threading.Tasks;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using GraphQLParser.AST;
using tanka.graphql.schema;
using tanka.graphql.sdl;

namespace tanka.graphql.benchmarks
{
    public static class Utils
    {
        public static ISchema InitializeSchema()
        {
            var events = new SingleValueEventChannel();
            var builder = new SchemaBuilder()
                .Sdl(Parser.ParseDocument(
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

            var resolvers = new ObjectTypeMap
            {
                {
                    "Query", new FieldResolversMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Mutation", new FieldResolversMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    "Subscription", new FieldResolversMap()
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