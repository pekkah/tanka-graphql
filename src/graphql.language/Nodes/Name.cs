using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public readonly struct Name : IEquatable<Name>
    {
        public readonly Location? Location;

        public readonly ReadOnlyMemory<byte> Value;

        public Name(in byte[] value, in Location? location)
        {
            Value = value;
            Location = location;
        }

        public ReadOnlySpan<byte> ValueSpan => Value.Span;

        public static implicit operator Name(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            return new Name(Encoding.UTF8.GetBytes(value), default);
        }

        public static implicit operator string(Name value)
        {
            if (value.ValueSpan.IsEmpty)
                return string.Empty;

            return Encoding.UTF8.GetString(value.ValueSpan);
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            string name = (string)this;
            return $"{name}{location}";
        }

        public bool Equals(Name other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is Name other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}