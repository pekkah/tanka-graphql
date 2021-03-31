using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental.Definitions
{
    public delegate object? CoerceValue(ExecutableSchema schema, object? value, TypeBase valueType);
}