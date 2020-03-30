using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public struct Name : IEquatable<Name>
    {
        public readonly Location? Location;

        public readonly ReadOnlyMemory<byte> Value;
        private string _value;

        public Name(in byte[] value, in Location? location = default)
        {
            Value = value;
            Location = location;
            _value = string.Empty;
        }

        public ReadOnlySpan<byte> ValueSpan => Value.Span;

        public static implicit operator Name(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            return new Name(Encoding.UTF8.GetBytes(value));
        }

        public static implicit operator string(in Name value)
        {
            return value.ToString();
        }

        public string AsString()
        {
            if (_value != string.Empty)
                return _value;

            return _value = Encoding.UTF8.GetString(ValueSpan);
        }

        public override string ToString()
        {
            return AsString();
        }

        public bool Equals(Name other)
        {
            return ValueSpan.SequenceEqual(other.ValueSpan);
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