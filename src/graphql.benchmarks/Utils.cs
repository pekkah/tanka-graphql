using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using fugu.graphql.resolvers;
using fugu.graphql.tools;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.benchmarks
{
    public static class Utils
    {
        public static Task<ISchema> InitializeSchema()
        {
            var query = new ObjectType("Query", new Fields
            {
                {"simple", new Field(ScalarType.String)}
            });

            var mutation = new ObjectType("Mutation", new Fields
            {
                {"simple", new Field(ScalarType.String)}
            });

            var subscription = new ObjectType("Subscription", new Fields
            {
                {"simple", new Field(ScalarType.String)}
            });

            var resolvers = new ResolverMap
            {
                {
                    query.Name, new FieldResolverMap
                    {
                        {"simple", context => Task.FromResult(Resolve.As("value"))}
                    }
                },
                {
                    mutation.Name, new FieldResolverMap
                    {
                        {"simple", context => Task.FromResult(Resolve.As("value"))}
                    }
                },
                {
                    subscription.Name, new FieldResolverMap()
                    {
                        {
                            "simple", 
                            async (context, unsubscribe) => Resolve.Stream(await SimpleValueBlock("value")), 
                            context => Task.FromResult(Resolve.As(context.ObjectValue))}
                    }
                }
            };

            var schema = SchemaTools.MakeExecutableSchemaAsync(
                new Schema(query, 
                    mutation,
                    subscription), 
                resolvers,
                resolvers);

            return schema;
        }

        private static async Task<ChannelReader<object>> SimpleValueBlock(string value)
        {
            var target = Channel.CreateUnbounded<object>();
            await target.Writer.WriteAsync(value);
            target.Writer.Complete();
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