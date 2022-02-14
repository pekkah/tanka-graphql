using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public abstract class ValueBase : INode
{
    public abstract NodeKind Kind { get; }
    public abstract Location? Location { get; }

    public static implicit operator ValueBase(string value)
    {
        var parser = Parser.Create(Encoding.UTF8.GetBytes(value));
        return parser.ParseValue(true);
    }

    public static implicit operator ValueBase(in ReadOnlySpan<byte> value)
    {
        var parser = Parser.Create(value);
        return parser.ParseValue(true);
    }
}