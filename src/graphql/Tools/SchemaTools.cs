using System.Collections.Generic;
using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Tools
{
    public static class SchemaTools
    {
        public static ISchema MakeExecutableSchema(
            SchemaBuilder builder,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            IReadOnlyDictionary<string, IValueConverter> converters = null,
            IReadOnlyDictionary<string, CreateDirectiveVisitor> directives = null)
        {
            // add converters
            if (converters != null)
                UseConverters(builder, converters);

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
            IResolverMap? resolvers = null,
            ISubscriberMap? subscribers = null,
            IReadOnlyDictionary<string, IValueConverter>? converters = null,
            IReadOnlyDictionary<string, CreateDirectiveVisitor>? directives = null)
        {
            // add converters
            if (converters != null)
                UseConverters(builder, converters);

            if (resolvers != null)
                builder.UseResolversAndSubscribers(resolvers, subscribers);

            if (directives != null)
                builder.ApplyDirectives(directives);

            var schema = builder.Build();
            var introspection = Introspect.Schema(schema);

            return new SchemaBuilder()
                .Merge(schema, introspection)
                .Build();
        }

        private static void UseConverters(SchemaBuilder builder,
            IReadOnlyDictionary<string, IValueConverter> converters)
        {
            foreach (var converter in converters)
            {
                // remove existing converter if exists
                builder.RemoveConverter(converter.Key);

                // include converter
                builder.Include(converter.Key, converter.Value);
            }
        }
    }
}