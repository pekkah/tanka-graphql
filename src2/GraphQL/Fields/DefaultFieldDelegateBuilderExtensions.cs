using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public static class DefaultFieldDelegateBuilderExtensions
{
    public static FieldDelegateBuilder UseDefault(this FieldDelegateBuilder builder)
    {
        return builder
            .UseDefaultArgumentCoercion()
            .UseResolver()
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

            return ResolveCore(context, resolver);

            static async ValueTask ResolveCore(ResolverContext context, Resolver resolver)
            {
                try
                {
                    await resolver(context);
                }
                catch (Exception e)
                {
                    e.Handle(context);
                }
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