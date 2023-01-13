using System;
using System.Threading.Tasks;

namespace Tanka.GraphQL.Experimental;

public static class ResolveAsResolverContextExtensions
{
    public static ValueTask ResolveAs<T>(this ResolverContext context, T? value)
    {
        context.ResolvedValue = value;
        return default;
    }

    public static ValueTask ResolveAsPropertyOf<T>(this ResolverContext context, Func<T, object?> valueFunc)
    {
        var source = context.ObjectValue is T objectValue ? objectValue : default;

        if (source is null)
        {
            context.ResolvedValue = null;
            return default;
        }

        var value = valueFunc(source);
        context.ResolvedValue = value;

        return default;
    }
}