using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.resolvers;
using tanka.graphql.schema;
using tanka.graphql.type;

namespace tanka.graphql.directives
{
    public static class SchemaBuilderExtensions
    {
        public static SchemaBuilder ApplyDirectives(
            this SchemaBuilder builder,
            Dictionary<string, CreateDirectiveVisitor> directiveFactories)
        {
            if (directiveFactories == null) 
                throw new ArgumentNullException(nameof(directiveFactories));

            foreach (var (directiveName, visitor) in directiveFactories.Select(d => (d.Key, d.Value(builder))))
            foreach (var objectType in builder.GetTypes<ObjectType>())
                builder.Connections(connections =>
                {
                    var fields = connections.GetFields(objectType)
                        .Where(field => field.Value.HasDirective(directiveName))
                        .ToList();

                    foreach (var field in fields)
                    {
                        var directive = field.Value.GetDirective(directiveName);

                        if (visitor.FieldDefinition == null)
                            continue;

                        var resolver = connections.GetOrAddResolver(objectType, field.Key)?.Build();
                        var subscriber = connections.GetOrAddSubscriber(objectType, field.Key)?.Build();
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
                            connections.Remove(objectType, field.Key);
                            continue;
                        }

                        // changed so remove and add
                        connections.Remove(objectType, field.Key);
                        connections.Include(objectType, new[]
                        {
                            new KeyValuePair<string, IField>(maybeSameField.Name, maybeSameField.Field)
                        });
                        connections.Include(objectType, maybeSameField.Name,
                            new ResolverBuilder(maybeSameField.Resolver));
                        connections.Include(objectType, maybeSameField.Name,
                            new SubscriberBuilder(maybeSameField.Subscriber));
                    }
                });

            return builder;
        }
    }
}