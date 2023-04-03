using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class NamedType : TypeBase
{
    public readonly Name Name;

    public NamedType(
        in Name name,
        in Location? location = default)
    {
        Name = name;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.NamedType;
    public override Location? Location { get; }

    public static implicit operator NamedType(string value)
    {
        var parser = Parser.Create(value);
        return parser.ParseNamedType();
    }
}