using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server
{
    public static class ResolverContextExtensions
    {
        public static TContext Use<TContext>(this ResolverContext resolverContext)
        {
            return resolverContext.Extension<ContextExtensionScope<TContext>>().Context;
        }
    }
}