using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

                if (source == null) return ResolveSync.As(null);

                var value = getValue(source);
                return ResolveSync.As(value);
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IEnumerable<object>> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return ResolveSync.As(null);

                var values = getValue(source);

                if (values == null)
                    return ResolveSync.As(null);

                return ResolveSync.As(values.Select(As));
            };
        }

        public static ISubscribeResult Subscribe<T>(EventChannel<T> eventChannel, CancellationToken unsubscribe)
        {
            return eventChannel.Subscribe(unsubscribe);
        }
    }
}