using System;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class Name : IEquatable<Name>
    {
        public readonly Location? Location;

        public readonly string Value;

        public Name(string value, in Location? location)
        {
            Value = value;
            Location = location;
        }

        public bool Equals(Name other)
        {
            return Value == other.Value;
        }

        public static implicit operator Name(string value)
        {
            return new Name(value, default);
        }

        public static implicit operator string(Name value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            var location = Location.Equals(default) ? Location.ToString() : string.Empty;
            return $"{Value}{location}";
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