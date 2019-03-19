using System.Linq;
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
            : this($"Cannot return null for non-nullable field '{type}.{field}'.", path, nodes)
        {
        }

        protected NullValueForNonNullException(string message, NodePath path, params ASTNode[] nodes) : base(message, nodes, path: path, locations:nodes.Select(n => n.Location))
        {
        }
    }
}