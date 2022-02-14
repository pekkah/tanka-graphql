using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class InputObjectDefinition : TypeDefinition
{
    public InputObjectDefinition(
        StringValue? description,
        in Name name,
        Directives? directives,
        InputFieldsDefinition? fields,
        in Location? location = default)
    {
        Description = description;
        Name = name;
        Directives = directives;
        Fields = fields;
        Location = location;
    }

    public StringValue? Description { get; }
    public override Directives? Directives { get; }
    public InputFieldsDefinition? Fields { get; }
    public override NodeKind Kind => NodeKind.InputObjectDefinition;
    public override Location? Location { get; }
    public override Name Name { get; }

    public static implicit operator InputObjectDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseInputObjectDefinition();
    }

    public static implicit operator InputObjectDefinition(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseInputObjectDefinition();
    }
}