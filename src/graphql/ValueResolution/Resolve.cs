using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.ValueResolution
{
    public static class Resolve
    {
        public static IResolverResult As(object? result)
        {
            return new CompleteValueResult(result);
        }

        public static IResolverResult As(ObjectDefinition type, object? result)
        {
            return new CompleteValueResult(result, type);
        }

        public static IResolverResult As(IEnumerable? result)
        {
            return new CompleteValueResult(result);
        }

        public static IResolverResult As(IEnumerable? result, Func<IResolverContext, object?, TypeDefinition> isTypeOf)
        {
            return new CompleteValueResult(result, isTypeOf);
        }

        public static Resolver PropertyOf<T>(Func<T, object?> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return ResolveSync.As(null);

                var value = getValue(source);
                return ResolveSync.As(value);
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IResolverContext, object?> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return ResolveSync.As(null);

                var value = getValue(source, context);
                return ResolveSync.As(value);
            };
        }

        public static Resolver PropertyOf<T>(Func<T, IEnumerable<object?>?> getValue)
        {
            return context =>
            {
                var source = context.ObjectValue is T objectValue ? objectValue : default;

                if (source == null) return ResolveSync.As(null);

                var values = getValue(source);

                if (values == null)
                    return ResolveSync.As(null);

                return ResolveSync.As(values);
            };
        }

        public static ISubscriberResult Subscribe<T>(EventChannel<T> eventChannel, CancellationToken unsubscribe)
        {
            return eventChannel.Subscribe(unsubscribe);
        }
    }
}