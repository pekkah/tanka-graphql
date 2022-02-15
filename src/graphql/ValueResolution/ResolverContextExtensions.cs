using System;
using System.Collections.Generic;

namespace Tanka.GraphQL.ValueResolution;

public static class ResolverContextExtensions
{
    public static T? GetArgument<T>(this IResolverContext context, string name)
    {
        if (!context.Arguments.TryGetValue(name, out var arg))
            throw new ArgumentOutOfRangeException(nameof(name), name,
                $"Field '{context.FieldName}' does not contain argument with name '{name}''");

        return (T?)arg;
    }


    /// <summary>
    ///     Read InputObject argument dictionary as object
    /// </summary>
    /// <remarks>
    ///     Experimental. This might go away anytime and be replaced with something better.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T? GetObjectArgument<T>(this IResolverContext context, string name)
        where T : IReadFromObjectDictionary, new()
    {
        var arg = context.GetArgument<IReadOnlyDictionary<string, object>>(name);

        if (arg is null)
            return default;

        var value = new T();
        value.Read(arg);
        return value;
    }

    /// <summary>
    ///     Read InputObject argument dictionary as object
    /// </summary>
    /// <remarks>
    ///     Experimental. This might go away anytime and be replaced with something better.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IEnumerable<T?> GetObjectArgumentList<T>(this IResolverContext context, string name)
        where T : IReadFromObjectDictionary, new()
    {
        var arg = context.GetArgument<IEnumerable<IReadOnlyDictionary<string, object>>>(name);

        if (arg is null)
            yield break;

        foreach (var item in arg)
        {
            if (item is null)
            {
                yield return default;
                continue;
            }

            var value = new T();
            value.Read(item);
            yield return value;
        }
    }

    public static T Extension<T>(this IResolverContext context) where T : IExtensionScope
    {
        return context.ExecutionContext.ExtensionsRunner.Extension<T>();
    }
}