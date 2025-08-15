using Tanka.GraphQL.Directives;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.TypeSystem;

/// <summary>
/// Middleware that handles directive visitors and type resolution
/// </summary>
public class ApplyDirectivesMiddleware : ISchemaBuildMiddleware
{
    public async Task<ISchema> InvokeAsync(ISchemaBuildContext context, SchemaBuildDelegate next)
    {
        var options = context.Options;

        var typeDefinitions = context.Builder.GetTypeDefinitions(context.Options.BuildTypesFromOrphanedExtensions)
            .ToList();

        RunDirectiveVisitors(
            typeDefinitions,
            options,
            context.Resolvers,
            context.Builder);


        return await next(context);
    }

    private void RunDirectiveVisitors(
        IList<TypeDefinition> typeDefinitions,
        SchemaBuildOptions options,
        ResolversBuilder resolvers,
        SchemaBuilder builder)
    {
        if (options.DirectiveVisitorFactories is null)
            return;

        var visitors = options.DirectiveVisitorFactories
            .Select(factory => new KeyValuePair<string, DirectiveVisitor>(
                factory.Key,
                factory.Value(builder)
            ));

        RunDirectiveVisitors(
            typeDefinitions,
            options,
            visitors,
            resolvers,
            builder);
    }

    private void RunDirectiveVisitors(
        IList<TypeDefinition> typeDefinitions,
        SchemaBuildOptions options,
        IEnumerable<KeyValuePair<string, DirectiveVisitor>> visitors,
        ResolversBuilder resolvers,
        SchemaBuilder schema)
    {
        var typeDefinitionList = typeDefinitions.ToList();
        if (typeDefinitionList.Count is 0)
            return;

        var visitorList = visitors.ToList();
        for (var typeIndex = 0; typeIndex < typeDefinitionList.Count; typeIndex++)
        {
            var typeDefinition = typeDefinitionList[typeIndex];
            foreach (var (directiveName, visitor) in visitorList)
            {
                if (visitor.TypeDefinition is not null)
                {
                    if (!typeDefinition.TryGetDirective(directiveName, out var directive))
                        continue;

                    var context = new DirectiveTypeVisitorContext(typeDefinition);

                    var maybeSameContext = visitor.TypeDefinition(directive, context);

                    // type removed
                    if (maybeSameContext?.TypeDefinition is null)
                    {
                        typeDefinitionList.RemoveAt(typeIndex);
                        typeIndex--; // Adjust index after removal
                        break;
                    }

                    typeDefinition = maybeSameContext.TypeDefinition;
                }

                if (typeDefinition is ObjectDefinition objectDefinition)
                {
                    if (visitor.FieldDefinition != null && objectDefinition.Fields is { Count: > 0 })
                    {
                        var fieldsChanged = false;
                        var fields = objectDefinition.Fields.ToList();
                        foreach (var fieldDefinition in objectDefinition.Fields)
                        {
                            if (!fieldDefinition.TryGetDirective(directiveName, out var directive))
                                continue;

                            var resolver = options.Resolvers?.GetResolver(typeDefinition.Name, fieldDefinition.Name);
                            var subscriber =
                                options.Subscribers?.GetSubscriber(typeDefinition.Name, fieldDefinition.Name);

                            var context = new DirectiveFieldVisitorContext(
                                fieldDefinition,
                                resolver,
                                subscriber
                            );

                            var maybeSameContext = visitor.FieldDefinition(directive, context);

                            // field not modified
                            if (maybeSameContext == context)
                            {
                                continue;
                            }

                            fieldsChanged = true;

                            // field removed
                            if (maybeSameContext is null)
                            {
                                fields.Remove(fieldDefinition);
                                continue;
                            }

                            if (maybeSameContext.Resolver is not null)
                                resolvers.ReplaceResolver(typeDefinition.Name, fieldDefinition.Name, new ResolverBuilder().Run(maybeSameContext.Resolver));


                            if (maybeSameContext.Subscriber is not null)
                                resolvers.ReplaceSubscriber(typeDefinition.Name, fieldDefinition.Name,
                                    new SubscriberBuilder().Run(maybeSameContext.Subscriber));
                        }

                        if (fieldsChanged)
                        {
                            typeDefinition = objectDefinition.WithFields(fields);
                            typeDefinitionList[typeIndex] = typeDefinition;
                        }
                    }
                }
            }
        }
    }
}