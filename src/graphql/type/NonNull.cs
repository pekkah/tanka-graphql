using System;

namespace tanka.graphql.type
{
    public class NonNull : IGraphQLType, IWrappingType, IEquatable<IGraphQLType>, IEquatable<NonNull>
    {
        public NonNull(IGraphQLType wrappedType)
        {
            WrappedType = wrappedType;
        }

        public bool Equals(IGraphQLType other)
        {
            return Equals((object) other);
        }

        public bool Equals(NonNull other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(WrappedType.Unwrap().Name, other.WrappedType.Unwrap().Name);
        }

        public string Name { get; } = null;

        public IGraphQLType WrappedType { get; }

        public override string ToString()
        {
            return $"{WrappedType}!";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NonNull) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^
                       (WrappedType != null ? WrappedType.GetHashCode() : 0);
            }
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