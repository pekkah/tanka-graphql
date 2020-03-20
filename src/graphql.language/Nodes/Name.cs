using System;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Name : IEquatable<Name>
    {
        public readonly Location? Location;

        public readonly byte[] Value;

        public ReadOnlySpan<byte> ValueSpan => Value;

        public Name(in byte[] value, in Location? location)
        {
            Value = value;
            Location = location;
        }

        public bool Equals(Name other)
        {
            if (ValueSpan.IsEmpty && !other.ValueSpan.IsEmpty)
                return false;

            return ValueSpan.SequenceEqual(other.ValueSpan);
        }

        public static implicit operator Name(string value)
        {
            return new Name(Encoding.UTF8.GetBytes(value), default);
        }

        public static implicit operator string(Name value)
        {
            return Encoding.UTF8.GetString(value.ValueSpan);
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            string name = this;
            return $"{name}{location}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Name other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Name left, Name right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Name left, Name right)
        {
            return !left.Equals(right);
        }
    }
}