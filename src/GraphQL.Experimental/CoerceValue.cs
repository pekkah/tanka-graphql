using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public delegate object? CoerceValue(ExecutableSchema schema, object? value, TypeBase valueType);
}