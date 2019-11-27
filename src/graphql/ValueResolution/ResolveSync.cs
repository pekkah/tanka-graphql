using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public static class ResolveSync
    {
        public static ValueTask<ISubscriberResult> Subscribe<T>(EventChannel<T> eventChannel,
            CancellationToken unsubscribe)
        {
            return new ValueTask<ISubscriberResult>(eventChannel.Subscribe(unsubscribe));
        }

        public static ValueTask<IResolverResult> As(object result)
        {
            return new ValueTask<IResolverResult>(new CompleteValueResult(result, null));
        }

        public static ValueTask<IResolverResult> As(string result)
        {
            return new ValueTask<IResolverResult>(new CompleteValueResult((object)result, null));
        }

        public static ValueTask<IResolverResult> As(ObjectType type, object result)
        {
            return new ValueTask<IResolverResult>(new CompleteValueResult(result, type));
        }

        public static ValueTask<IResolverResult> As(IEnumerable result)
        {
            return new ValueTask<IResolverResult>(new CompleteValueResult(result, null));
        }
    }
}