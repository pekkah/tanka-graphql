using System;
using System.Collections.Generic;

namespace tanka.graphql.type
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
                Include(scalarType);

            IncludeDirective(type.DirectiveType.Include);
            IncludeDirective(type.DirectiveType.Skip);
        }

        public SchemaBuilder(ISchema from): this()
        {
            foreach (var namedType in from.QueryTypes<INamedType>())
            {
                if (IsPredefinedType<INamedType>(namedType.Name, out _))
                    continue;

                switch (namedType)
                {
                    case ObjectType objectType:
                        Include(objectType);
                        IncludeFields(objectType, from.GetFields(objectType.Name));
                        break;
                    case InterfaceType interfaceType:
                        Include(interfaceType);
                        IncludeFields(interfaceType, from.GetFields(interfaceType.Name));
                        break;
                    case InputObjectType inputType:
                        Include(inputType);
                        IncludeInputFields(inputType, from.GetInputFields(inputType.Name));
                        break;
                    default:
                        Include(namedType);
                        break;
                }
            }

            foreach (var directiveType in from.QueryDirectives())
            {
                if (_directives.ContainsKey(directiveType.Name))
                    continue;

                IncludeDirective(directiveType);
            }
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

            _fields[owner.Name].Add(fieldName, new Field(to, new Args(args), 
                new Meta(description, directives: directives)));
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

        public SchemaBuilder Enum(string name,
            out EnumType enumType,
            IEnumerable<DirectiveInstance> directives = null,
            params (string value, Meta meta)[] values)
        {
            enumType = new EnumType(name, new EnumValues(values), new Meta(directives: directives));
            _types.Add(name, enumType);
            return this;
        }

        public ISchema Build()
        {
            foreach (var lateBuildAction in _lateBuild) lateBuildAction(this);

            return new SchemaGraph(
                _types,
                _fields,
                _inputFields,
                _directives);
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

        public SchemaBuilder IncludeDirective(DirectiveType directiveType)
        {
            _directives.Add(directiveType.Name, directiveType);
            return this;
        }

        public SchemaBuilder Include(INamedType type)
        {
            _types.Add(type.Name, type);
            return this;
        }

        public SchemaBuilder IncludeFields(ComplexType owner, IEnumerable<KeyValuePair<string, IField>> fields)
        {
            foreach (var field in fields)
            {
                if (!_fields.ContainsKey(owner.Name))
                    _fields[owner.Name] = new Dictionary<string, IField>();

                _fields[owner.Name].Add(field.Key, field.Value);
            }

            return this;
        }

        protected SchemaBuilder IncludeInputFields(InputObjectType owner,
            IEnumerable<KeyValuePair<string, InputObjectField>> fields)
        {
            foreach (var field in fields)
            {
                if (!_fields.ContainsKey(owner.Name))
                    _inputFields[owner.Name] = new Dictionary<string, InputObjectField>();

                _inputFields[owner.Name].Add(field.Key, field.Value);
            }

            return this;
        }

        public bool IsPredefinedField(ComplexType owner, string fieldName, out IField field)
        {
            if (_fields.TryGetValue(owner.Name, out var fields))
            {
                if (fields.TryGetValue(fieldName, out field))
                {
                    return true;
                }
            }

            field = null;
            return false;
        }
    }
}