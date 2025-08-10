using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public static class DefaultFieldDelegateBuilderExtensions
{
    public static FieldDelegateBuilder UseDefault(this FieldDelegateBuilder builder)
    {
        return builder
            .UseDefaultArgumentCoercion()
            .UseResolver()
            .UseStreamHandling()
            .RunCompleteValue();
    }

    public static FieldDelegateBuilder UseDefaultArgumentCoercion(this FieldDelegateBuilder builder)
    {
        return builder.Use(next => context =>
        {
            context.ArgumentValues = ArgumentCoercion.CoerceArgumentValues(
                context.QueryContext.Schema,
                context.ObjectDefinition,
                context.Fields.First(),
                context.QueryContext.CoercedVariableValues);

            return next(context);
        });
    }

    public static FieldDelegateBuilder UseResolver(this FieldDelegateBuilder builder)
    {
        return builder.Use(next => context =>
        {
            context.QueryContext.RequestCancelled.ThrowIfCancellationRequested();

            var path = context.Path;
            ISchema schema = context.QueryContext.Schema;
            var objectDefinition = context.ObjectDefinition;
            var fieldName = context.FieldName;


            if (context.Field is null)
                throw new QueryException(
                    $"Object '{objectDefinition.Name}' does not have field '{fieldName}'")
                {
                    Path = context.Path
                };

            // __typename hack
            if (fieldName == "__typename")
            {
                context.CompletedValue = objectDefinition.Name.Value;
                return ValueTask.CompletedTask;
            }

            Resolver? resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}")
                {
                    Path = path
                };

            return ResolveCore(context, resolver, next);

            static async ValueTask ResolveCore(ResolverContext context, Resolver resolver, FieldDelegate next)
            {
                try
                {
                    await resolver(context);
                    await next(context);
                }
                catch (Exception e)
                {
                    e.Handle(context);
                }
            }
        });
    }

    public static FieldDelegateBuilder UseStreamHandling(this FieldDelegateBuilder builder)
    {
        return builder.Use(next => async context =>
        {
            // Check if this field has @stream directive and pass initialCount to value completion
            if (context.Field is not null
                && context.FieldMetadata?.TryGetValue("stream", out var streamValue) == true
                && streamValue is Directive streamDirective)
            {
                var initialCount = Ast.GetDirectiveArgumentValue(streamDirective, "initialCount", context.QueryContext.CoercedVariableValues) as int? ?? 0;
                var label = Ast.GetDirectiveArgumentValue(streamDirective, "label", context.QueryContext.CoercedVariableValues) as string;

                // Complete value with stream parameters
                await context.QueryContext.CompleteValueAsync(context, context.Field.Type, context.Path, initialCount, label);
            }
            else
            {
                // Continue to next middleware (RunCompleteValue will handle regular completion)
                await next(context);
            }
        });
    }

    public static FieldDelegateBuilder RunCompleteValue(this FieldDelegateBuilder builder)
    {
        return builder.Use(_ => async context =>
        {
            if (context.Field is not null)
                await context.QueryContext.CompleteValueAsync(context, context.Field.Type, context.Path);
        });
    }

}