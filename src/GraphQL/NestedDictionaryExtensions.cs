namespace Tanka.GraphQL;

public static class NestedDictionaryExtensions
{
    public static IReadOnlyDictionary<string, object?>? NestedOrNull(this IReadOnlyDictionary<string, object?> parent,
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
}