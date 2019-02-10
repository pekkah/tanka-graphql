using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using tanka.graphql.graph;
using tanka.graphql.type;
using Xunit;

namespace tanka.graphql.tests.type
{
    public class SchemaBuilder
    {
        private readonly Dictionary<string, Dictionary<string, IField>> _fields =
            new Dictionary<string, Dictionary<string, IField>>();

        private readonly Dictionary<string, INamedType> _types =
            new Dictionary<string, INamedType>();

        public SchemaBuilder Object(
            string name,
            out ObjectType definition,
            string description = null)
        {
            definition = new ObjectType(name, new Fields());
            _types.Add(name, definition);
            return this;
        }

        public SchemaBuilder Query(
            out ObjectType query)
        {
            Object("Query", out query);
            return this;
        }


        public SchemaBuilder Field(
            ComplexType owner,
            string fieldName,
            ComplexType to,
            string description = null)
        {
            if (!_fields.ContainsKey(owner.Name)) 
                _fields[owner.Name] = new Dictionary<string, IField>();

            _fields[owner.Name].Add(fieldName, new Field(to));
            return this;
        }

        public SchemaBuilder Field(
            ComplexType owner,
            string fieldName,
            ScalarType type,
            string description = null)
        {
            if (!_fields.ContainsKey(owner.Name)) 
                _fields[owner.Name] = new Dictionary<string, IField>();

            _fields[owner.Name].Add(fieldName, new Field(type));
            return this;
        }

        public ISchema Build()
        {
            return new Schema2(
                new ReadOnlyDictionary<string, INamedType>(_types),
                new ReadOnlyDictionary<string, Dictionary<string, IField>>(_fields));
        }
    }

    public class Schema2 : ISchema
    {
        private readonly IReadOnlyDictionary<string, Dictionary<string, IField>> _fields;
        private readonly IReadOnlyDictionary<string, INamedType> _types;

        public Schema2(
            IReadOnlyDictionary<string, INamedType> types,
            IReadOnlyDictionary<string, Dictionary<string, IField>> fields)
        {
            _types = types;
            _fields = fields;
        }

        public bool IsInitialized { get; } = true;

        public ObjectType Subscription => GetNamedType<ObjectType>("Mutation");

        public ObjectType Query => GetNamedType<ObjectType>("Query");

        public ObjectType Mutation => GetNamedType<ObjectType>("Mutation");

        public INamedType GetNamedType(string name)
        {
            var type = _types[name];
            return (INamedType)WithFields(type);
        }

        private IType WithFields(IType type)
        {
            if (type is ObjectType objectType)
                return objectType.WithFields(_fields[objectType.Name].ToArray());

            if (type is InterfaceType interfaceType)
                return interfaceType.WithFields(_fields[interfaceType.Name].ToArray());

            return type;
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T> filter = null) where T : IType
        {
            if (filter == null)
                return _types.Select(t => t.Value)
                    .Select(WithFields)
                    .OfType<T>()
                    .AsQueryable();

            return _types.Select(t => t.Value)
                .Select(WithFields)
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

    public class SchemaFactsV2
    {
        [Fact]
        public void Build_with_circular_reference_between_two_objects()
        {
            /* Given */
            var builder = new SchemaBuilder();

            /* When */
            var schema = builder
                .Object("Object1", out var obj1)
                .Object("Object2", out var obj2)
                .Field(obj1, "obj1-obj2", obj2)
                .Field(obj2, "obj2-obj1", obj1)
                .Field(obj1, "scalar", ScalarType.Int)
                .Query(out var query)
                .Field(query, "query-obj1", obj1)
                .Build();

            /* Then */
            var object1 = schema.GetNamedType<ObjectType>(obj1.Name);
            var object1ToObject2 = object1.GetField("obj1-obj2");

            var object2 = schema.GetNamedType<ObjectType>(obj2.Name);
            var object2ToObject1 = object2.GetField("obj2-obj1");

            Assert.Equal(object1, object2ToObject1.Type);
            Assert.Equal(object2, object1ToObject2.Type);
        }
    }
}