﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using fugu.graphql.introspection;
using fugu.graphql.type;

namespace fugu.graphql.tools
{
    public static class SchemaTools
    {
        public static async Task<ISchema> MakeExecutableSchemaAsync(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            IEnumerable<SchemaVisitorFactory> visitors = null)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            AddResolversAndSubscribers(schema, resolvers, subscribers);

            if (visitors != null)
                foreach (var visitorFactory in visitors)
                {
                    var visitor = visitorFactory(schema, resolvers, subscribers);
                    await visitor.VisitAsync();
                }

            return schema;
        }

        public static async Task<ISchema> MakeExecutableSchemaWithIntrospection(
            ISchema schema,
            IResolverMap resolvers,
            ISubscriberMap subscribers = null,
            IEnumerable<SchemaVisitorFactory> visitors = null)
        {
            if (!schema.IsInitialized)
                await schema.InitializeAsync();

            var introspection = await Introspection.ExamineAsync(schema);
            var executable = await MakeExecutableSchemaAsync(schema, resolvers, subscribers);

            var withIntrospection = MergeTool.MergeSchemas(executable, introspection, (l, r) => r.Field);

            if (visitors != null)
                foreach (var visitorFactory in visitors)
                {
                    var visitor = visitorFactory(withIntrospection, resolvers, subscribers);
                    await visitor.VisitAsync();
                }

            await withIntrospection.InitializeAsync();
            return withIntrospection;
        }

        private static void AddResolversAndSubscribers(ISchema schema, IResolverMap resolvers,
            ISubscriberMap subscribers)
        {
            foreach (var type in schema.QueryTypes<ComplexType>())
            foreach (var field in type.Fields)
            {
                field.Value.Resolve = field.Value.Resolve ?? resolvers.GetResolver(type, field);

                if (field.Value.Resolve == null)
                {
                    Debug.WriteLine($"Could not find resolver for {type.Name}:{field.Key}");
                }

                if (subscribers != null)
                    field.Value.Subscribe = field.Value.Subscribe ?? subscribers.GetSubscriber(type, field);
            }
        }
    }
}