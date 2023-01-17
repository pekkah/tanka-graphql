using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueResolution;

public class NullValueForNonNullTypeException : CompleteValueException
{
    public NullValueForNonNullTypeException(
        string type,
        string field,
        NodePath path,
        params INode[] nodes)
        : base($"Cannot return null for non-nullable field '{type}.{field}'.", path, nodes)
    {
    }
}