using System.Collections;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.introspection
{
    internal static class SyncWrap
    {
        public static Task<IResolveResult> Sync(object result)
        {
            return Task.FromResult(Resolve.As(result));
        }

        public static Task<IResolveResult> Sync(IEnumerable result)
        {
            return Task.FromResult(Resolve.As(result));
        }
    }
}