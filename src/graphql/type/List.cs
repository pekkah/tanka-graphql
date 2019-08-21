using System;

namespace tanka.graphql.type
{
    public class List : IWrappingType, IEquatable<List>
    {
        public IType OfType { get; }

        public List(IType wrappedType)
        {
            OfType = wrappedType ?? throw new ArgumentNullException(nameof(wrappedType));
        }

        public override string ToString()
        {
            return $"[{OfType}]";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((List) obj);
        }

        public bool Equals(List other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(OfType, other.OfType);
        }

        public override int GetHashCode()
        {
            return (OfType != null ? OfType.GetHashCode() : 0);
        }

        public static bool operator ==(List left, List right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(List left, List right)
        {
            return !Equals(left, right);
        }
    }
}