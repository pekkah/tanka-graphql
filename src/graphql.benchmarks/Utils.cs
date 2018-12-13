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
                            (context, unsubscribe) => Task.FromResult(Resolve.Stream(SimpleValueBlock("value"))), 
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