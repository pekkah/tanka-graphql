using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem;

public static class TypeExtensions
{
    public static NamedType Unwrap(this TypeBase type)
    {
        return type switch
        {
            NonNullType NonNullType => Unwrap(NonNullType.OfType),
            ListType list => Unwrap(list.OfType),
            _ => (NamedType)type
        };
    }
}