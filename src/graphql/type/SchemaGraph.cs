using System;
using System.Collections.Generic;
using System.Linq;

namespace tanka.graphql.type
{
    public class SchemaGraph : ISchema
    {
        private readonly Dictionary<string, DirectiveType> _directiveTypes;
        private readonly Dictionary<string, Dictionary<string, IField>> _fields;
        private readonly Dictionary<string, Dictionary<string, InputObjectField>> _inputFields;
        private readonly Dictionary<string, INamedType> _types;

        public SchemaGraph(
            Dictionary<string, INamedType> types,
            Dictionary<string, Dictionary<string, IField>> fields,
            Dictionary<string, Dictionary<string, InputObjectField>> inputFields,
            Dictionary<string, DirectiveType> directiveTypes)
        {
            _types = types;
            _fields = fields;
            _inputFields = inputFields;
            _directiveTypes = directiveTypes;
            Query = GetNamedType<ObjectType>("Query") ?? throw new ArgumentNullException(
                        nameof(types),
                        $"Could not find root type 'Query' from given types");
            Mutation = GetNamedType<ObjectType>("Mutation");
            Subscription = GetNamedType<ObjectType>("Subscription");
        }

        public bool IsInitialized { get; } = true;

        public ObjectType Subscription { get; }

        public ObjectType Query { get; }

        public ObjectType Mutation { get; }

        public INamedType GetNamedType(string name)
        {
            if (_types.TryGetValue(name, out var type))
            {
                return type;
            }

            return null;
        }

        public IField GetField(string type, string name)
        {
            if (_fields.TryGetValue(type, out var fields))
                if (fields.TryGetValue(name, out var field))
                    return field;

            return null;
        }

        public IEnumerable<KeyValuePair<string, IField>> GetFields(string type)
        {
            if (_fields.TryGetValue(type, out var fields))
            {
                return fields;
            }

            return Enumerable.Empty<KeyValuePair<string, IField>>();
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : INamedType
        {
            if (filter == null)
                return _types.Select(t => t.Value)
                    .OfType<T>()
                    .AsQueryable();

            return _types.Select(t => t.Value)
                .OfType<T>()
                .Where(t => filter(t))
                .AsQueryable();
        }

        public DirectiveType GetDirective(string name)
        {
            return _directiveTypes[name];
        }

        public IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null)
        {
            return _directiveTypes.Select(v => v.Value).AsQueryable();
        }

        public IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(string type)
        {
            if (_inputFields.TryGetValue(type, out var fields))
            {
                return fields;
            }

            return Enumerable.Empty<KeyValuePair<string, InputObjectField>>();
        }

        public InputObjectField GetInputField(string type, string name)
        {
            return _inputFields[type][name];
        }

        public T GetNamedType<T>(string name) where T : INamedType
        {
            return (T) GetNamedType(name);
        }
    }
}