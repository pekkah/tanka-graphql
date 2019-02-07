using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public class ObjectType : ComplexType, IDescribable
    {
        private readonly Dictionary<string, InterfaceType> _interfaces = new Dictionary<string, InterfaceType>();

        public ObjectType(
            string name,
            Fields fields,
            Meta meta = null,
            IEnumerable<InterfaceType> implements = null)
        {
            Name = name;
            Meta = meta ?? new Meta();

            foreach (var field in fields)
                AddField(field.Key, field.Value);

            if (implements != null)
                foreach (var interfaceType in implements)
                    _interfaces[interfaceType.Name] = interfaceType;
        }

        public IEnumerable<InterfaceType> Interfaces => _interfaces.Values;

        public Meta Meta { get; }

        public override string Name { get; }
        public string Description => Meta.Description;

        public bool Implements(InterfaceType interfaceType)
        {
            return _interfaces.ContainsKey(interfaceType.Name);
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public KeyValuePair<string, IField> GetFieldWithKey(string name)
        {
            return Fields.SingleOrDefault(f => f.Key == name);
        }
    }
}