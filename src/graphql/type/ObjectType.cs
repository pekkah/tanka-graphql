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

        public ObjectType WithEachInterface(
            Func<InterfaceType, InterfaceType> withInterface)
        {
            if (!Interfaces.Any())
                return this;

            var interfaces = Interfaces.Select(withInterface)
                .ToList();

            return new ObjectType(
                Name,
                new Fields(Fields),
                Meta,
                interfaces
            );
        }

        public ObjectType WithEachField(
            Func<KeyValuePair<string, IField>, KeyValuePair<string, IField>> withField)
        {
            var deletedFields = new List<KeyValuePair<string, IField>>();
            var addedFields = new List<KeyValuePair<string, IField>>();

            foreach (var field in Fields)
            {
                var maybeNewField = withField(field);

                if (Equals(maybeNewField, field))
                    continue;

                if (Equals(maybeNewField, default(KeyValuePair<string, IField>)))
                {
                    deletedFields.Add(field);
                    continue;
                }

                addedFields.Add(maybeNewField);
                deletedFields.Add(field);
            }

            return ExcludeFields(deletedFields.ToArray())
                .IncludeFields(addedFields.ToArray());
        }

        public ObjectType ExcludeFields(
            params KeyValuePair<string, IField>[] excludedFields)
        {
            if (!excludedFields.Any())
                return this;

            return WithFields(
                Fields
                    .Where(field => !excludedFields.Contains(field))
                    .ToArray()
            );
        }

        public ObjectType WithFields(
            params KeyValuePair<string, IField>[] fields)
        {
            return new ObjectType(
                Name,
                new Fields(fields),
                Meta,
                Interfaces
            );
        }

        public ObjectType IncludeFields(
            params KeyValuePair<string, IField>[] includedFields)
        {
            if (!includedFields.Any())
                return this;

            return new ObjectType(
                Name,
                new Fields(Fields.Concat(includedFields)),
                Meta,
                Interfaces
            );
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