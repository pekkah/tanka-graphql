using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public class ValidationError
{
    private readonly List<INode> _nodes = new();

    public ValidationError(string message, params INode[] nodes)
    {
        Message = message;
        _nodes.AddRange(nodes);
    }

    public ValidationError(string code, string message, IEnumerable<INode> nodes)
        : this(message, nodes?.ToArray() ?? Array.Empty<INode>())
    {
        Code = code;
    }

    public ValidationError(string code, string message, INode node)
        : this(code, message, new[] { node })
    {
    }

    public string Code { get; set; }

    public string Message { get; set; }

    public IEnumerable<INode> Nodes => _nodes;

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
                if (node.Location != null)
                    builder.Append($"{node.Kind}@{node.Location}");
                else
                    builder.Append($"{node.Kind}");

                builder.Append(", ");
            }
        }

        return builder.ToString().Trim().TrimEnd(',');
    }

    public ExecutionError ToError()
    {
        return new ExecutionError
        {
            Message = ToString(),
            Locations = Nodes.Where(n => n.Location != null).Select(n => n.Location.Value).ToList(),
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