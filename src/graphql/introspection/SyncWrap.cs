using System;
using System.Collections;
using System.Threading.Tasks;
using tanka.graphql.resolvers;

namespace tanka.graphql.introspection
{
    [Obsolete]
    internal static class SyncWrap
    {
        public static ValueTask<IResolveResult> Sync(object result)
        {
            return new ValueTask<IResolveResult>(Resolve.As(result));
        }

        public static ValueTask<IResolveResult> Sync(IEnumerable result)
        {
            return new ValueTask<IResolveResult>(Resolve.As(result));
        }
    }
}