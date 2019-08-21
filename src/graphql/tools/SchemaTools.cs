using System.Collections.Generic;
using tanka.graphql.directives;
using tanka.graphql.introspection;
using tanka.graphql.resolvers;
using tanka.graphql.schema;
using tanka.graphql.type;

namespace tanka.graphql.tools
{
    public static class SchemaTools
    {
        public static ISchema MakeExecutableSchema(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            Dictionary<string, CreateDirectiveVisitor> directives = null)
        {
            // bind resolvers
            builder.UseResolversAndSubscribers(resolvers, subscribers);

            // execute directives
            if (directives != null) builder.ApplyDirectives(directives);

            return builder.Build();
        }

        public static ISchema MakeExecutableSchema(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null)
        {
            return MakeExecutableSchema(
                new SchemaBuilder().Import(schema),
                resolvers,
                subscribers);
        }

        public static ISchema MakeExecutableSchemaWithIntrospection(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            Dictionary<string, CreateDirectiveVisitor> directives = null)
        {
            builder.UseResolversAndSubscribers(resolvers, subscribers);

            if (directives != null)
                builder.ApplyDirectives(directives);

            var schema = builder.Build();
            var introspection = Introspect.Schema(schema);

            return new SchemaBuilder()
                .Merge(schema, introspection)
                .Build();
        }
    }
}