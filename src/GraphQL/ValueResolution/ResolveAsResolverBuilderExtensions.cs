namespace Tanka.GraphQL.ValueResolution;

public static class ResolveAsResolverBuilderExtensions
{
    public static ResolverBuilder ResolveAs<T>(this ResolverBuilder builder, T? value)
    {
        builder.Run(ctx => ctx.ResolveAs(value));
        return builder;
    }

    public static ResolverBuilder ResolveAsPropertyOf<T>(this ResolverBuilder builder, Func<T, object?> valueFunc)
    {
        builder.Run(ctx => ctx.ResolveAsPropertyOf<T>(valueFunc));
        return builder;
    }

    public static SubscriberBuilder ResolveAsStream<T>(this SubscriberBuilder builder, Func<CancellationToken, IAsyncEnumerable<T?>> generator)
    {
        builder.Run((ctx, unsubscribe) =>
        {
            ctx.ResolvedValue = Wrap(generator(unsubscribe));
            return default;
        });
        return builder;
    }

    private static async IAsyncEnumerable<object?> Wrap<T>(IAsyncEnumerable<T> generator)
    {
        await foreach (var o in generator)
        {
            yield return o;
        }
    }
}