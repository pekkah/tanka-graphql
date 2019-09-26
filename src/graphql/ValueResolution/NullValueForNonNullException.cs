using GraphQLParser.AST;
using Tanka.GraphQL.Execution;

namespace Tanka.GraphQL.ValueResolution
{
    public class NullValueForNonNullException : CompleteValueException
    {
        public NullValueForNonNullException(
            string type,
            string field,
            NodePath path,
            params ASTNode[] nodes)
            : base($"Cannot return null for non-nullable field '{type}.{field}'.", path, nodes)
        {
        }
    }
}