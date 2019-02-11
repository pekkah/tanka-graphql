using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public class ObjectType : ComplexType, IDescribable, IEquatable<ObjectType>
    {
        private readonly Dictionary<string, InterfaceType> _interfaces = new Dictionary<string, InterfaceType>();

        public ObjectType(
            string name,
            Meta meta = null,
            IEnumerable<InterfaceType> implements = null)
        :base(name)
        {
            Meta = meta ?? new Meta();

            if (implements != null)
                foreach (var interfaceType in implements)
                    _interfaces[interfaceType.Name] = interfaceType;
        }

        public IEnumerable<InterfaceType> Interfaces => _interfaces.Values;

        public Meta Meta { get; }
        
        public string Description => Meta.Description;

        public bool Implements(InterfaceType interfaceType)
        {
            return _interfaces.ContainsKey(interfaceType.Name);
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public bool Equals(ObjectType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectType) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(ObjectType left, ObjectType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectType left, ObjectType right)
        {
            return !Equals(left, right);
        }
    }
}