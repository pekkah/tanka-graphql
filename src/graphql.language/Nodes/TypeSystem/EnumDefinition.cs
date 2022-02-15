using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class EnumDefinition : TypeDefinition
{
    public EnumDefinition(
        StringValue? description,
        in Name name,
        Directives? directives,
        EnumValuesDefinition? values,
        in Location? location = default)
    {
        Description = description;
        Name = name;
        Directives = directives;
        Values = values;
        Location = location;
    }

    public StringValue? Description { get; }
    public override Directives? Directives { get; }
    public override NodeKind Kind => NodeKind.EnumDefinition;
    public override Location? Location { get; }
    public override Name Name { get; }
    public EnumValuesDefinition? Values { get; }

    public static implicit operator EnumDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseEnumDefinition();
    }

    public static implicit operator EnumDefinition(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseEnumDefinition();
    }
}