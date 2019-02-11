using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using tanka.graphql.type;

namespace tanka.graphql.typeSystem
{
    public class SchemaBuilder
    {
        private readonly Dictionary<string, DirectiveType> _directives = new Dictionary<string, DirectiveType>();

        private readonly Dictionary<string, Dictionary<string, IField>> _fields =
            new Dictionary<string, Dictionary<string, IField>>();

        private readonly Dictionary<string, Dictionary<string, InputObjectField>> _inputFields =
            new Dictionary<string, Dictionary<string, InputObjectField>>();

        private readonly List<Action<SchemaBuilder>> _lateBuild = new List<Action<SchemaBuilder>>();

        private readonly Dictionary<string, INamedType> _types =
            new Dictionary<string, INamedType>();

        public SchemaBuilder()
        {
            foreach (var scalarType in ScalarType.Standard)
                _types.Add(scalarType.Name, scalarType);
        }

        public SchemaBuilder Object(
            string name,
            out ObjectType definition,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new ObjectType(
                name,
                new Meta(description, directives: directives),
                interfaces);

            _types.Add(name, definition);
            return this;
        }

        public SchemaBuilder Interface(
            string name,
            out InterfaceType definition,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new InterfaceType(
                name,
                new Meta(description, directives: directives));

            _types.Add(name, definition);
            return this;
        }

        public SchemaBuilder InputObject(
            string name,
            out InputObjectType definition,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new InputObjectType(name, new Meta(description, directives: directives));
            _types.Add(name, definition);
            return this;
        }

        public SchemaBuilder Query(
            out ObjectType query)
        {
            Object("Query", out query);
            return this;
        }

        public SchemaBuilder Mutation(
            out ObjectType mutation)
        {
            Object("Mutation", out mutation);
            return this;
        }

        public SchemaBuilder Subscription(
            out ObjectType subscription)
        {
            Object("Subscription", out subscription);
            return this;
        }

        public SchemaBuilder Field(
            ComplexType owner,
            string fieldName,
            IType to,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params (string Name, IType Type, object DefaultValue, string Description)[] args)
        {
            if (!_fields.ContainsKey(owner.Name))
                _fields[owner.Name] = new Dictionary<string, IField>();

            _fields[owner.Name].Add(fieldName, new Field(to, new Args(args)));
            return this;
        }

        public SchemaBuilder InputField(
            InputObjectType owner,
            string fieldName,
            IType to,
            object defaultValue = null,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            if (!_inputFields.ContainsKey(owner.Name))
                _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

            _inputFields[owner.Name].Add(
                fieldName,
                new InputObjectField(to, new Meta(description, directives: directives), defaultValue));

            return this;
        }

        public SchemaBuilder Enum(
            string name,
            out EnumType enumType,
            params (string value, Meta meta)[] values)
        {
            enumType = new EnumType(name, new EnumValues(values));
            _types.Add(name, enumType);
            return this;
        }

        public ISchema Build()
        {
            foreach (var lateBuildAction in _lateBuild) lateBuildAction(this);

            return new SchemaGraph(
                new ReadOnlyDictionary<string, INamedType>(_types),
                new ReadOnlyDictionary<string, ReadOnlyDictionary<string, IField>>(_fields),
                new ReadOnlyDictionary<string, ReadOnlyDictionary<string, InputObjectField>>(_inputFields),
                new ReadOnlyDictionary<string, DirectiveType>(_directives));
        }

        public SchemaBuilder LateBuild(Action<SchemaBuilder> lateBuild)
        {
            _lateBuild.Add(lateBuild);
            return this;
        }

        public SchemaBuilder PredefinedScalar(string name, out ScalarType scalarType)
        {
            if (!_types.TryGetValue(name, out scalarType))
                throw new ArgumentOutOfRangeException(
                    $"Could not find scalar '{name}' from known types");

            return this;
        }

        public SchemaBuilder DirectiveType(
            string name,
            out DirectiveType directiveType,
            IEnumerable<DirectiveLocation> locations,
            params (string Name, IType Type, object DefaultValue, string Description)[] args)
        {
            directiveType = new DirectiveType(name, locations, new Args(args));
            _directives.Add(name, directiveType);
            return this;
        }

        //todo: predefined is bit misleading term to use here
        public bool IsPredefinedType<T>(string name, out T namedType)
            where T : INamedType
        {
            return _types.TryGetValue(name, out namedType);
        }

        public SchemaBuilder Union(
            string name,
            out UnionType unionType,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params ObjectType[] possibleTypes)
        {
            unionType = new UnionType(name, possibleTypes, new Meta(description, directives: directives));
            _types.Add(name, unionType);
            return this;
        }
    }
}