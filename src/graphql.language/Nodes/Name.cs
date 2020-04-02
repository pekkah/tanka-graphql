using System;

namespace Tanka.GraphQL.Language.Nodes
{
    public readonly struct Name : IEquatable<Name>
    {
        public Location? Location { get; }

        public readonly string Value;

        public Name(string value, in Location? location = default)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Location = location;
        }

        public static implicit operator Name(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            return new Name(value);
        }

        public static implicit operator string(in Name value)
        {
            return value.ToString();
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(Name other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Name other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public static bool operator ==(in Name left, in Name right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in Name left, in Name right)
        {
            return !left.Equals(right);
        }
    }
}