using System.Diagnostics.CodeAnalysis;

using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem;

public static class TypeDictionaryExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, TypeDefinition> types, string key,
        [NotNullWhen(true)] out T? type)
        where T : TypeDefinition
    {
        if (types.TryGetValue(key, out var value))
        {
            type = (T)value;
            return true;
        }

        type = default;
        return false;
    }
}