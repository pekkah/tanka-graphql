using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Channels;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.ValueResolution;

public static class ResolveSync
{
    public static ValueTask<IAsyncEnumerable<object?>> Subscribe<T>(EventChannel<T> eventChannel,
        CancellationToken unsubscribe)
    {
        throw new NotImplementedException();
    }

    public static ValueTask<object?> As(object? result)
    {
        return new ValueTask<object?>(result);
    }

    public static ValueTask<object?> As(string? result)
    {
        return new ValueTask<object?>(result);
    }

    public static ValueTask<object?> As(ObjectDefinition type, object? result)
    {
        throw new NotImplementedException();
    }

    public static ValueTask<object?> As(IEnumerable? result)
    {
        return new ValueTask<object?>(result);
    }
}