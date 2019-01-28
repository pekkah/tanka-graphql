using System;

namespace tanka.graphql.type
{
    public class NonNull : IWrappingType, IEquatable<NonNull>
    {
        public NonNull(IType wrappedType)
        {
            WrappedType = wrappedType;
        }

        public IType WrappedType { get; }

        public override string ToString()
        {
            return $"{WrappedType}!";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NonNull) obj);
        }

        public bool Equals(NonNull other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(WrappedType, other.WrappedType);
        }

        public override int GetHashCode()
        {
            return (WrappedType != null ? WrappedType.GetHashCode() : 0);
        }

        public static bool operator ==(NonNull left, NonNull right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NonNull left, NonNull right)
        {
            return !Equals(left, right);
        }
    }
}