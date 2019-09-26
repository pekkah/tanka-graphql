using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphQLParser.AST;

namespace Tanka.GraphQL.Validation
{
    public class ValidationError
    {
        private readonly List<ASTNode> _nodes = new List<ASTNode>();

        public ValidationError(string message, params ASTNode[] nodes)
        {
            Message = message;
            _nodes.AddRange(nodes);
        }

        public ValidationError(string code, string message, IEnumerable<ASTNode> nodes)
            : this(message, nodes.ToArray())
        {
            Code = code;
        }

        public ValidationError(string code, string message, ASTNode node)
            : this(code, message, new[] {node})
        {
        }

        public string Message { get; set; }

        public IEnumerable<ASTNode> Nodes => _nodes;

        public string Code { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Message);

            if (Nodes.Any())
            {
                if (!Message.EndsWith("."))
                    builder.Append(". ");

                builder.Append("At ");

                foreach (var node in Nodes)
                {
                    builder.Append($"{node.Kind}@{node.Location.Start}:{node.Location.End}");
                    builder.Append(", ");
                }
            }

            return builder.ToString().Trim().TrimEnd(',');
        }

        public ExecutionError ToError()
        {
            return new ExecutionError(ToString())
            {
                Locations = Nodes.Select(n => n.Location).ToList(),
                Extensions = new Dictionary<string, object>
                {
                    {
                        "doc", new
                        {
                            section = Code
                        }
                    }
                }
            };
        }
    }
}