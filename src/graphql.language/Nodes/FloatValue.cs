using System;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class FloatValue : Value, INode
    {
        public override NodeKind Kind => NodeKind.FloatValue;
        public readonly bool IsExponent;
        public override Location? Location {get;}
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
    }
}