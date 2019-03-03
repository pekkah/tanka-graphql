using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql.resolvers;
using tanka.graphql.tools;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.benchmarks
{
    public static class Utils
    {
        public static Task<ISchema> InitializeSchema()
        {
            var builder = new SchemaBuilder();
            builder.Query(out var query)
                .Connections(connect => connect
                .Field(query, "simple", ScalarType.String));

            builder.Mutation(out var mutation)
                .Connections(connect => connect
                .Field(mutation, "simple", ScalarType.String));

            builder.Subscription(out var subscription)
                .Connections(connect => connect
                .Field(subscription, "simple", ScalarType.String));

            var resolvers = new ResolverMap
            {
                {
                    query.Name, new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    mutation.Name, new FieldResolverMap
                    {
                        {"simple", context => new ValueTask<IResolveResult>(Resolve.As("value"))}
                    }
                },
                {
                    subscription.Name, new FieldResolverMap()
                    {
                        {
                            "simple", 
                            (context, unsubscribe) => new ValueTask<ISubscribeResult>(Resolve.Stream(SimpleValueBlock("value"))), 
                            context => new ValueTask<IResolveResult>(Resolve.As(context.ObjectValue))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchemaAsync(
                builder.Build(), 
                resolvers,
                resolvers);

            return schema;
        }

        private static ISourceBlock<object> SimpleValueBlock(string value)
        {
            var target = new BufferBlock<string>();
            target.Post(value);
            return target;
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