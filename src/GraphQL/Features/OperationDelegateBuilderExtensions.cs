namespace Tanka.GraphQL.Features;

public static class OperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder AddFeature<TFeature>(this OperationDelegateBuilder builder, TFeature feature)
    {
        return builder.Use(next => context =>
        {
            context.Features.Set(feature);
            return next(context);
        });
    }
}