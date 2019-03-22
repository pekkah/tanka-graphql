using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using tanka.graphql.channels;
using tanka.graphql.type;

namespace tanka.graphql.resolvers
{
    public static class Resolve
    {
        public static IResolveResult As(object result)
        {
            return new ResolveResult(result);
        }

        public static IResolveResult As(ObjectType type, object result)
        {
            return new ResolveResult(type, result);
        }

        public static IResolveResult As(IEnumerable result)
        {
            return new ResolveResult(result);
        }

        public static Resolver PropertyOf<T>(Func<T, object> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return new ValueTask<IResolveResult>(As(null));

                var value = getValue(source);
                return new ValueTask<IResolveResult>(As(value));
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IEnumerable<object>> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return new ValueTask<IResolveResult>(As(null));

                var values = getValue(source);

                if (values == null)
                    return new ValueTask<IResolveResult>(As(null));

                return new ValueTask<IResolveResult>(As(values.Select(As)));
            };
        }

        public static ISubscribeResult Subscribe<T>(EventChannel<T> eventChannel, CancellationToken unsubscribe)
        {
            return eventChannel.Subscribe(unsubscribe);
        }

        [Obsolete("Use SubscribeResult")]
        public static ISubscribeResult Stream(ISourceBlock<object> reader)
        {
            var result = new SubscribeResult();
            var _ = Task.Run(async () =>
            {
                while (!reader.Completion.IsCompleted)
                {
                    var item = await reader.ReceiveAsync();
                    await result.WriteAsync(item);
                }

                result.TryComplete();
            });

            return result;
        }
    }

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