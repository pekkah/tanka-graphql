using System.Collections;

namespace Tanka.GraphQL;

public static class NestedDictionaryExtensions
{
    public static IReadOnlyDictionary<string, object?>? NestedOrNull(
        this IReadOnlyDictionary<string, object?> parent,
        string key)
    {
        if (!parent.TryGetValue(key, out var nested))
        {
            return null;
        }

        if (nested is not IReadOnlyDictionary<string, object?> nestedDictionary)
        {
            throw new InvalidOperationException(
                $"Nested value of key '{key}' is not an {nameof(IReadOnlyDictionary<string, object?>)}.");
        }

        return nestedDictionary;
    }

    public static object? Select(this IReadOnlyDictionary<string, object?>? er, params object[] path)
    {
        IReadOnlyDictionary<string, object?>? currentObject = er;
        object? result = null;
        foreach (var segment in path)
        {
            if (segment is string stringSegment)
            {
                if (currentObject == null)
                    return null;

                if (currentObject.ContainsKey(stringSegment))
                    result = currentObject[stringSegment];
                else
                    result = null;
            }

            if (segment is int intSegment)
            {
                if (result is IEnumerable enumerable)
                {
                    var count = 0;
                    foreach (var elem in enumerable)
                    {
                        if (count == intSegment)
                        {
                            result = elem;
                            break;
                        }

                        count++;
                    }
                }
                else
                {
                    result = null;
                }
            }

            if (result is Dictionary<string, object?> child)
                currentObject = child;
        }

        return result;
    }
}