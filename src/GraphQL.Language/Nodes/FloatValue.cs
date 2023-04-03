using System;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class FloatValue : ValueBase, INode
{
    public readonly bool IsExponent;
    public readonly ReadOnlyMemory<byte> Value;

    public FloatValue(
        in byte[] value,
        bool isExponent,
        in Location? location = default)
    {
        Value = value;
        IsExponent = isExponent;
        Location = location;
    }

    public ReadOnlySpan<byte> ValueSpan => Value.Span;
    public override NodeKind Kind => NodeKind.FloatValue;
    public override Location? Location { get; }
}