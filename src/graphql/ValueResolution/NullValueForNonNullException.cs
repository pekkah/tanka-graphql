
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueResolution
{
    public class NullValueForNonNullException : CompleteValueException
    {
        public NullValueForNonNullException(
            string type,
            string field,
            NodePath path,
            params INode[] nodes)
            : base($"Cannot return null for non-nullable field '{type}.{field}'.", path, nodes)
        {
        }
    }
}