using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public static class SchemaBuilderExtensions
    {
        public static SchemaBuilder UseResolversAndSubscribers(
            this SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            foreach (var type in builder.GetTypes<ObjectType>())
                builder.Connections(connections =>
                {
                    foreach (var field in connections.GetFields(type))
                    {
                        var resolver = resolvers.GetResolver(type.Name, field.Key);

                        if (resolver != null)
                            connections.GetOrAddResolver(type, field.Key)
                                .Run(resolver);

                        var subscriber = subscribers?.GetSubscriber(type.Name, field.Key);

                        if (subscriber != null)
                            connections.GetOrAddSubscriber(type, field.Key)
                                .Run(subscriber);
                    }
                });

            return builder;
        }
    }
}