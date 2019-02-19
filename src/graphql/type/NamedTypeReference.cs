using System;

namespace tanka.graphql.type
{
    public class NamedTypeReference : INamedType, IEquatable<NamedTypeReference>
    {
        public NamedTypeReference(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }

        public bool Equals(NamedTypeReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TypeName, other.TypeName);
        }

        public string Name => $"->{TypeName}";

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NamedTypeReference) obj);
        }

        public override int GetHashCode()
        {
            return TypeName != null ? TypeName.GetHashCode() : 0;
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