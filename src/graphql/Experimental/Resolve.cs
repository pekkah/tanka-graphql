using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.ValueResolution;

public static class Resolve
{
    public static object? As(object? result)
    {
        return result;
    }

    public static object? As(ObjectDefinition type, object? result)
    {
        return result;
    }

    public static object? As(IEnumerable? result)
    {
        return result;
    }

    public static object? As(IEnumerable? result, Func<object?, object?, TypeDefinition> isTypeOf)
    {
        return result;
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

    public static Resolver PropertyOf<T>(Func<T, ResolverContext, object?> getValue)
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

    public static IAsyncEnumerable<object?> Subscribe<T>(EventChannel<T> eventChannel, CancellationToken unsubscribe)
    {
        throw new NotImplementedException();
    }
}