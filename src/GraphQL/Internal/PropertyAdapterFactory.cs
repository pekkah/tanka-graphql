using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

//using FastExpressionCompiler.LightExpression;
//using LightExpression = FastExpressionCompiler.LightExpression.Expression;

namespace Tanka.GraphQL.Internal;

public static class PropertyAdapterFactory
{
    private static readonly ConcurrentDictionary<Type, object> AdapterCache = new();

    public static IReadOnlyDictionary<string, IPropertyAdapter<T>> GetPropertyAdapters<T>()
    {
        return (IReadOnlyDictionary<string, IPropertyAdapter<T>>)AdapterCache.GetOrAdd(typeof(T), _ => CreateAdapters<T>());
    }

    private static IReadOnlyDictionary<string, IPropertyAdapter<T>> CreateAdapters<T>()
    {
        var type = typeof(T);
        return type.GetProperties().Select(property =>
        {
            Func<T, object> getter = null;
            var getMethod = property.GetGetMethod();
            if (getMethod != null)
            {
                getter = GetProperty<T, object>(property);
            }

            Action<T, object> setter = null;
            var setMethod = property.GetSetMethod();
            if (setMethod != null)
            {
                setter = SetProperty<T, object>(property);
            }

            return (IPropertyAdapter<T>)new PropertyAdapter<T>(property.Name, getter, setter);
        }).ToDictionary(p => p.Name, p => p);
    }

    public static Func<TTarget, TProperty>? GetProperty<TTarget, TProperty>(PropertyInfo property)
    {
        var target = Expression.Parameter(property.DeclaringType, "target");
        var method = property.GetGetMethod();

        var callGetMethod = Expression.Call(target, method);

        var lambda = method.ReturnType == typeof(TProperty)
            ? Expression.Lambda<Func<TTarget, TProperty>>(callGetMethod, target)
            : Expression.Lambda<Func<TTarget, TProperty>>(Expression.Convert(callGetMethod, typeof(TProperty)),
                target);

        return lambda.Compile();
    }

    public static Action<TTarget, TProperty>? SetProperty<TTarget, TProperty>(PropertyInfo property)
    {
        var target = Expression.Parameter(property.DeclaringType, "target");
        var value = Expression.Parameter(typeof(TProperty), "value");
        var propertyType = property.PropertyType;

        var method = property.SetMethod;

        if (method == null)
            return null;

        if (value.Type == propertyType)
        {

            var callSetMethod = Expression.Call(target, method, value);

            var lambda = Expression.Lambda<Action<TTarget, TProperty>>(callSetMethod, target, value);

            return lambda.Compile();
        }
        else
        {
            UnaryExpression conversion;
            if (propertyType.IsEnum)
            {
                var type = Expression.Constant(propertyType);
                var parseEnum = Expression.Call(null, _parseEnumValue, type, Expression.Call(value, _toString), Expression.Constant(true));
                conversion = Expression.Convert(parseEnum, propertyType);
            }
            else
                conversion = Expression.Convert(value, propertyType);


            var callSetMethod = Expression.Call(target, method, conversion);
            var lambda = Expression.Lambda<Action<TTarget, TProperty>>(callSetMethod, target, value);

            return lambda.Compile();
        }
    }


    private static readonly MethodInfo _parseEnumValue = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(mi =>
        {
            if (mi.IsGenericMethodDefinition)
                return false;

            if (mi.Name is not nameof(Enum.Parse))
                return false;

            var parameters = mi.GetParameters();

            if (parameters.Length != 3)
                return false;

            if (parameters[1].ParameterType != typeof(string))
                return false;

            if (parameters[2].ParameterType != typeof(bool))
                return false;

            return true;
        });

    private static readonly MethodInfo _toString = typeof(object).GetMethod("ToString");

    private static readonly MethodInfo _getEnumStringValue = typeof(Enum).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(mi => mi is { Name: nameof(Enum.GetName), IsGenericMethod: false });

}