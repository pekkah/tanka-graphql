using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.resolvers;

namespace tanka.graphql.type
{
    public class SchemaGraph : ISchema
    {
        private readonly Dictionary<string, DirectiveType> _directiveTypes;
        private readonly Dictionary<string, Dictionary<string, Resolver>> _resolvers;
        private readonly Dictionary<string, Dictionary<string, Subscriber>> _subscribers;
        private readonly Dictionary<string, Dictionary<string, IField>> _fields;
        private readonly Dictionary<string, Dictionary<string, InputObjectField>> _inputFields;
        private readonly Dictionary<string, INamedType> _types;
        private readonly DirectiveList _directives;

        public SchemaGraph(
            Dictionary<string, INamedType> types,
            Dictionary<string, Dictionary<string, IField>> fields,
            Dictionary<string, Dictionary<string, InputObjectField>> inputFields,
            Dictionary<string, DirectiveType> directiveTypes,
            Dictionary<string, Dictionary<string, Resolver>> resolvers,
            Dictionary<string, Dictionary<string, Subscriber>> subscribers,
            IEnumerable<DirectiveInstance> directives = null)
        {
            _types = types;
            _fields = fields;
            _inputFields = inputFields;
            _directiveTypes = directiveTypes;
            _resolvers = resolvers;
            _subscribers = subscribers;
            Query = GetNamedType<ObjectType>("Query") ?? throw new ArgumentNullException(
                        nameof(types),
                        $"Could not find root type 'Query' from given types");
            Mutation = GetNamedType<ObjectType>("Mutation");
            Subscription = GetNamedType<ObjectType>("Subscription");
            _directives = new DirectiveList(directives);
        }

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

        public DirectiveType GetDirectiveType(string name)
        {
            if (_directiveTypes.TryGetValue(name, out var directive))
            {
                return directive;
            }

            return null;
        }

        public IQueryable<DirectiveType> QueryDirectiveTypes(Predicate<DirectiveType> filter = null)
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
            if (_inputFields.TryGetValue(type, out var fields))
                if (fields.TryGetValue(name, out var field))
                    return field;

            return null;
        }

        public IEnumerable<ObjectType> GetPossibleTypes(IAbstractType abstractType)
        {
            return QueryTypes<ObjectType>(abstractType.IsPossible);
        }

        public Resolver GetResolver(string type, string fieldName)
        {
            if (_resolvers.TryGetValue(type, out var fields))
                if (fields.TryGetValue(fieldName, out var field))
                    return field;

            return null;
        }

        public Subscriber GetSubscriber(string type, string fieldName)
        {
            if (_subscribers.TryGetValue(type, out var fields))
                if (fields.TryGetValue(fieldName, out var field))
                    return field;

            return null;
        }

        public T GetNamedType<T>(string name) where T : INamedType
        {
            return (T) GetNamedType(name);
        }

        public IEnumerable<DirectiveInstance> Directives => _directives;

        public DirectiveInstance GetDirective(string name)
        {
            return _directives.GetDirective(name);
        }
    }
}