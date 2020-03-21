using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public readonly struct StringValue : IValue
    {
        public readonly Location? Location;
        public readonly ReadOnlyMemory<byte> Value;

        public StringValue(
            in byte[] value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }

        public ReadOnlySpan<byte> ValueSpan => Value.Span;

        public static implicit operator StringValue(string value)
        {
            return new StringValue(Encoding.UTF8.GetBytes(value), default);
        }

        public static implicit operator string(StringValue value)
        {
            return Encoding.UTF8.GetString(value.ValueSpan);
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            return $"\"{Value}\"{location}";
        }
    }
}