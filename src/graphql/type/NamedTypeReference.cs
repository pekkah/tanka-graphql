using System;

namespace fugu.graphql.type
{
    public class NamedTypeReference : IGraphQLType, IEquatable<NamedTypeReference>
    {
        public NamedTypeReference(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }

        public string Name { get; } = null;

        public bool Equals(NamedTypeReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TypeName, other.TypeName) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamedTypeReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TypeName != null ? TypeName.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(NamedTypeReference left, NamedTypeReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamedTypeReference left, NamedTypeReference right)
        {
            return !Equals(left, right);
        }
    }
}