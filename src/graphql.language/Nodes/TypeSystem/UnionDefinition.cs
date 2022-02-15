using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem;

public sealed class UnionDefinition : TypeDefinition
{
    public UnionDefinition(
        StringValue? description,
        in Name name,
        Directives? directives,
        UnionMemberTypes? members,
        in Location? location = default)
    {
        Description = description;
        Name = name;
        Directives = directives;
        Members = members;
        Location = location;
    }

    public StringValue? Description { get; }

    public override Directives? Directives { get; }
    public override NodeKind Kind => NodeKind.UnionDefinition;

    public override Location? Location { get; }

    public UnionMemberTypes? Members { get; }

    public override Name Name { get; }

    public static implicit operator UnionDefinition(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseUnionDefinition();
    }

    public static implicit operator UnionDefinition(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseUnionDefinition();
    }
}