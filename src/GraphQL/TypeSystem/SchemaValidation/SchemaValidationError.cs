using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public class SchemaValidationError
{
    public SchemaValidationError(string code, string message, INode? node = null)
    {
        Code = code;
        Message = message;
        Node = node;
    }

    public SchemaValidationError(string code, string message, IEnumerable<INode> nodes)
    {
        Code = code;
        Message = message;
        Nodes = nodes.ToList();
    }

    public string Code { get; }
    public string Message { get; }
    public INode? Node { get; }
    public IReadOnlyList<INode>? Nodes { get; }

    public override string ToString()
    {
        return $"{Code}: {Message}";
    }
}