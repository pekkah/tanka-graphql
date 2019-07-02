using GraphQLParser.AST;
using tanka.graphql.execution;

namespace tanka.graphql.resolvers
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