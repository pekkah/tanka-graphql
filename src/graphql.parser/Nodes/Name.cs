using System;

namespace Tanka.GraphQL.Language.Nodes
{
    public class Name : IEquatable<Name>
    {
        public readonly Location Location;

        public readonly string Value;

        public Name(in string value, in Location location)
        {
            Value = value;
            Location = location;
        }

        public bool Equals(Name? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Name) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Name? left, Name? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Name? left, Name? right)
        {
            return !Equals(left, right);
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
    }
}