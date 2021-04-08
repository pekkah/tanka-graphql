using System;
using System.Threading.Tasks;
using Tanka.GraphQL.Experimental.Definitions;

namespace Tanka.GraphQL.Experimental
{
    public static class Resolve
    {
        public static ResolveFieldValue As(Func<object?, object?> getValue)
        {
            return (context, objectDefinition, objectValue, fieldName, variableValues, path, cancellationToken) =>
            {
                var parent = objectValue;
                var value = getValue(parent);

                return new ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)>((value, null));
            };
        }
    }

    public static class Resolve<T>
    {
        public static ResolveFieldValue As(Func<T?, object?> getValue)
        {
            return (context, objectDefinition, objectValue, fieldName, variableValues, path, cancellationToken) =>
            {
                var parent = (T?) objectValue;

                var value = getValue(parent);

                return new ValueTask<(object? Value, ResolveAbstractType? ResolveAbstractType)>((value, null));
            };
        }
    }
}