using System.Collections.Generic;
using GraphQLParser.AST;

namespace fugu.graphql.validation
{
    public class ValidationError
    {
        private readonly List<ASTNode> _nodes = new List<ASTNode>();

        public ValidationError(string message, params ASTNode[] nodes)
        {
            Message = message;
            _nodes.AddRange(nodes);
        }

        public string Message { get; set; }

        public IEnumerable<ASTNode> Nodes => _nodes;
    }
}