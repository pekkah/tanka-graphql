using tanka.graphql.introspection;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
    //todo(pekka): review API
    public static class SchemaTools
    {
        public static ISchema MakeExecutableSchema(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            UseResolversAndSubscribers(builder, resolvers, subscribers);
            return builder.Build();
        }

        public static ISchema MakeExecutableSchema(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            return MakeExecutableSchema(
                new SchemaBuilder(schema),
                resolvers,
                subscribers);
        }

        public static void UseResolversAndSubscribers(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            foreach (var type in builder.VisitTypes<ObjectType>())
                builder.Connections(connections =>
                {
                    foreach (var field in connections.VisitFields(type))
                    {
                        var resolver = resolvers.GetResolver(type.Name, field.Key);

                        if (resolver != null)
                            connections.GetResolver(type, field.Key)
                                .Use(resolver);

                        var subscriber = subscribers?.GetSubscriber(type.Name, field.Key);

                        if (subscriber != null)
                            connections.GetSubscriber(type, field.Key)
                                .Use(subscriber);
                    }
                });
        }

        public static ISchema MakeExecutableSchemaWithIntrospection(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            return MakeExecutableSchemaWithIntrospection(
                new SchemaBuilder(schema),
                resolvers,
                subscribers);
        }

        public static ISchema MakeExecutableSchemaWithIntrospection(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            UseResolversAndSubscribers(builder, resolvers, subscribers);

            var schema = builder.Build();
            var introspection = Introspect.Schema(schema);

            var withIntrospection = MergeTool
                .MergeSchemas(
                    schema,
                    introspection);

            return withIntrospection;
        }
    }
}