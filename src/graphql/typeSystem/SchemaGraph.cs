using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.typeSystem
{
    public class SchemaGraph : ISchema
    {
        private readonly IReadOnlyDictionary<string, DirectiveType> _directiveTypes;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IField>> _fields;
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, InputObjectField>> _inputFields;
        private readonly IReadOnlyDictionary<string, INamedType> _types;

        public SchemaGraph(IReadOnlyDictionary<string, INamedType> types,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IField>> fields,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, InputObjectField>> inputFields,
            IReadOnlyDictionary<string, DirectiveType> directiveTypes)
        {
            _types = types;
            _fields = fields;
            _inputFields = inputFields;
            _directiveTypes = directiveTypes;
        }

        public bool IsInitialized { get; } = true;

        public ObjectType Subscription => GetNamedType<ObjectType>("Mutation");

        public ObjectType Query => GetNamedType<ObjectType>("Query");

        public ObjectType Mutation => GetNamedType<ObjectType>("Mutation");

        public INamedType GetNamedType(string name)
        {
            var type = _types[name];
            return type;
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
            return _fields[type];
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IType
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
            return null;
        }

        public IQueryable<DirectiveType> QueryDirectives(Predicate<DirectiveType> filter = null)
        {
            return null;
        }

        public T GetNamedType<T>(string name) where T : INamedType
        {
            return (T) GetNamedType(name);
        }
    }
}