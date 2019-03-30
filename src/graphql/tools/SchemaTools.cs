using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.introspection;
using tanka.graphql.resolvers;
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
            UseResolversAndSubscribers(builder, resolvers, subscribers);

            // execute directives
            if (directives != null)
                UseDirectives(builder, directives);

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
                            connections.GetOrAddResolver(type, field.Key)
                                .Run(resolver);

                        var subscriber = subscribers?.GetSubscriber(type.Name, field.Key);

                        if (subscriber != null)
                            connections.GetSubscriber(type, field.Key)
                                .Run(subscriber);
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
            ISubscriberMap subscribers = null,
            Dictionary<string, CreateDirectiveVisitor> directives = null)
        {
            UseResolversAndSubscribers(builder, resolvers, subscribers);

            if (directives != null)
                UseDirectives(builder, directives);

            var schema = builder.Build();
            var introspection = Introspect.Schema(schema);

            var withIntrospection = MergeTool
                .MergeSchemas(
                    schema,
                    introspection);

            return withIntrospection;
        }

        public static void UseDirectives(SchemaBuilder builder,
            Dictionary<string, CreateDirectiveVisitor> directiveFactories)
        {
            if (directiveFactories == null) throw new ArgumentNullException(nameof(directiveFactories));

            foreach (var (directiveName, visitor) in directiveFactories.Select(d => (d.Key, d.Value(builder))))
            foreach (var objectType in builder.VisitTypes<ObjectType>())
                builder.Connections(connections =>
                {
                    var fields = connections.VisitFields(objectType)
                        .Where(field => field.Value.HasDirective(directiveName))
                        .ToList();

                    foreach (var field in fields)
                    {
                        var directive = field.Value.GetDirective(directiveName);

                        if (visitor.FieldDefinition == null)
                            continue;

                        var resolver = connections.GetOrAddResolver(objectType, field.Key)?.Build();
                        var subscriber = connections.GetSubscriber(objectType, field.Key)?.Build();
                        var fieldDefinition = new DirectiveFieldVisitorContext(
                            field.Key,
                            field.Value,
                            resolver,
                            subscriber);

                        var maybeSameField = visitor.FieldDefinition(directive, fieldDefinition);

                        // field not modified
                        if (maybeSameField == fieldDefinition)
                            continue;

                        // field removed
                        if (maybeSameField == null)
                        {
                            connections.RemoveField(objectType, field.Key);
                            continue;
                        }

                        // changed so remove and add
                        connections.RemoveField(objectType, field.Key);
                        connections.IncludeFields(objectType, new[]
                        {
                            new KeyValuePair<string, IField>(maybeSameField.Name, maybeSameField.Field)
                        });
                        connections.IncludeResolver(objectType, maybeSameField.Name,
                            new ResolverBuilder(maybeSameField.Resolver));
                        connections.IncludeSubscriber(objectType, maybeSameField.Name,
                            new SubscriberBuilder(maybeSameField.Subscriber));
                    }
                });
        }
    }
}