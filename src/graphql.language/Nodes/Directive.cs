using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class Directive : INode
{
    public readonly Arguments? Arguments;
    public readonly Name Name;

    public Directive(
        in Name name,
        Arguments? arguments,
        in Location? location = default)
    {
        Name = name;
        Arguments = arguments;
        Location = location;
    }

    public NodeKind Kind => NodeKind.Directive;
    public Location? Location { get; }

    public static implicit operator Directive(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseDirective(true);
    }

    public static implicit operator Directive(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseDirective(true);
    }
}