using System.Collections;
using System.Threading.Tasks;
using fugu.graphql.resolvers;

namespace fugu.graphql.introspection
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