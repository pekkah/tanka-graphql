using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class StringValue : ValueBase, INode
    {
        public override NodeKind Kind => NodeKind.StringValue;
        public override Location? Location {get;}
        public readonly ReadOnlyMemory<byte> Value;

        public StringValue(
            in byte[] value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }

        public ReadOnlySpan<byte> ValueSpan => Value.Span;

        public static implicit operator StringValue(string value)
        {
            return new StringValue(Encoding.UTF8.GetBytes(value), default);
        }

        public static implicit operator string?(StringValue? value)
        {
            return value?.ToString();
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(ValueSpan);
        }
    }
}