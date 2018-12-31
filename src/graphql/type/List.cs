using System;

namespace tanka.graphql.type
{
    public class List : IGraphQLType, IWrappingType, IEquatable<List>
    {
        public string Name { get; } = null;

        public IGraphQLType WrappedType { get; }

        public List(IGraphQLType wrappedType)
        {
            WrappedType = wrappedType ?? throw new ArgumentNullException(nameof(wrappedType));
        }

        public override string ToString()
        {
            return $"[{WrappedType}]";
        }

        public bool Equals(List other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(WrappedType.Unwrap().Name, other.WrappedType.Unwrap().Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((List) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (WrappedType != null ? WrappedType.GetHashCode() : 0);
            }
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