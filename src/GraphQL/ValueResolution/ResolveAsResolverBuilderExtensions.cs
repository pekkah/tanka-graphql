namespace Tanka.GraphQL.ValueResolution;

public static class ResolveAsResolverBuilderExtensions
{
    public static ResolverBuilder ResolveAs<T>(this ResolverBuilder builder, T? value)
    {
        builder.Run(ctx => ctx.ResolveAs(value));
        return builder;
    }
}