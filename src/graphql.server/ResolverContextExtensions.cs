using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server;

public static class ResolverContextExtensions
{
    /// <summary>
    ///     Use context extension from execution scope
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TContext ContextExtension<TContext>(this IResolverContext context)
    {
        return context.Extension<ContextExtensionScope<TContext>>().Context;
    }

    /// <summary>
    ///     Get service from execution scope service provider
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TService Use<TService>(this IResolverContext context)
    {
        return context.ContextExtension<IServiceScope>()
            .ServiceProvider
            .GetRequiredService<TService>();
    }
}