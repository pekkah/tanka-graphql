using System.Collections.Concurrent;
using FastExpressionCompiler.LightExpression;
using LightExpression = FastExpressionCompiler.LightExpression.Expression;

namespace Tanka.GraphQL.Internal;

public static class PropertyAdapterFactory
{
    private static readonly ConcurrentDictionary<Type, object> AdapterCache = new();

    public static IReadOnlyDictionary<string, PropertyAdapter<T>> GetPropertyAdapters<T>()
    {
        return (IReadOnlyDictionary<string, PropertyAdapter<T>>)AdapterCache.GetOrAdd(typeof(T), _ => CreateAdapters<T>());
    }

    private static IReadOnlyDictionary<string, PropertyAdapter<T>> CreateAdapters<T>()
    {
        var type = typeof(T);
        return type.GetProperties().Select(property =>
        {
            var eventLogCustomType = property.DeclaringType;
            var propertyType = property.PropertyType;

            var instance = LightExpression.Parameter(eventLogCustomType);

            Func<T, object> getter = null;
            var getMethod = property.GetGetMethod();
            if (getMethod != null)
                getter = LightExpression.Lambda<Func<T, object>>(LightExpression.Call(instance, getMethod), instance)
                    .CompileFast();

            Action<T, object> setter = null;
            var setMethod = property.GetSetMethod();
            if (setMethod != null)
            {
                var parameter = LightExpression.Parameter(propertyType);
                setter = LightExpression
                    .Lambda<Action<T, object>>(LightExpression.Call(instance, setMethod, parameter), instance,
                        parameter).CompileFast();
            }

            return new PropertyAdapter<T>(property.Name, getter, setter);
        }).ToDictionary(p => p.Name, p => p);
    }
}