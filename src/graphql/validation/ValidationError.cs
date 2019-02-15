using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public class ValidationError
    {
        private readonly List<ASTNode> _nodes = new List<ASTNode>();

        public ValidationError(string message, params ASTNode[] nodes)
        {
            Message = message;
            _nodes.AddRange(nodes);
        }

        public ValidationError(int code, string message, params ASTNode[] nodes)
            : this(message, nodes)
        {
            Code = code;
        }

        public string Message { get; set; }

        public IEnumerable<ASTNode> Nodes => _nodes;

        public int Code { get; set; } = -1;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Message);

            if (Nodes.Any())
            {
                builder.Append(" at ");

                foreach (var node in Nodes)
                {
                    builder.Append($"{node.Kind}@{node.Location.Start}:{node.Location.End}");
                    builder.Append(", ");
                }
            }

            return builder.ToString().TrimEnd(',');
        }
    }
}