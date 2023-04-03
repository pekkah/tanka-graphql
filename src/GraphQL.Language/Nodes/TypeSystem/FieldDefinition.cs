using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class FieldDefinition : INode
{
    public FieldDefinition(StringValue? description,
        in Name name,
        ArgumentsDefinition? arguments,
        TypeBase type,
        Directives? directives = default,
        in Location? location = default)
    {
        Description = description;
        Name = name;
        Arguments = arguments;
        Type = type;
        Directives = directives;
        Location = location;
    }

    public ArgumentsDefinition? Arguments { get; }

    public StringValue? Description { get; }
    public Directives? Directives { get; }
    public Name Name { get; }
    public TypeBase Type { get; }
    public NodeKind Kind => NodeKind.FieldDefinition;
    public Location? Location { get; }

    public static implicit operator FieldDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseFieldDefinition();
    }

    public static implicit operator FieldDefinition(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseFieldDefinition();
    }
}