using System;
using System.Collections.Generic;

namespace tanka.graphql.introspection
{
    // ReSharper disable once InconsistentNaming
    public class __Type : IEquatable<__Type>
    {
        public __TypeKind? Kind { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<__Field> Fields { get; set; }

        public List<__Type> Interfaces { get; set; }

        public List<__Type> PossibleTypes { get; set; }

        public List<__EnumValue> EnumValues { get; set; }

        public List<__InputValue> InputFields { get; set; }

        public __Type OfType { get; set; }

        public bool Equals(__Type other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Kind == other.Kind && string.Equals(Name, other.Name);
        }

        public override string ToString()
        {
            return $"{Kind} {Name}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((__Type) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Kind.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(__Type left, __Type right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(__Type left, __Type right)
        {
            return !Equals(left, right);
        }
    }
}