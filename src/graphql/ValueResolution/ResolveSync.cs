using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public static class ResolveSync
    {
        public static ValueTask<ISubscribeResult> Subscribe<T>(EventChannel<T> eventChannel,
            CancellationToken unsubscribe)
        {
            return new ValueTask<ISubscribeResult>(eventChannel.Subscribe(unsubscribe));
        }

        public static ValueTask<IResolveResult> As(object result)
        {
            return new ValueTask<IResolveResult>(new ResolveResult(result));
        }

        public static ValueTask<IResolveResult> As(string result)
        {
            return new ValueTask<IResolveResult>(new ResolveResult((object)result));
        }

        public static ValueTask<IResolveResult> As(ObjectType type, object result)
        {
            return new ValueTask<IResolveResult>(new ResolveResult(type, result));
        }

        public static ValueTask<IResolveResult> As(IEnumerable result)
        {
            return new ValueTask<IResolveResult>(new ResolveResult(result));
        }
    }
}