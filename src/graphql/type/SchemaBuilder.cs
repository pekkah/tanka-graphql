using System;
using System.Collections.Generic;
using tanka.graphql.type.converters;

namespace tanka.graphql.type
{
    public class SchemaBuilder
    {
        private readonly Dictionary<string, DirectiveType> _directives = new Dictionary<string, DirectiveType>();

        private readonly List<Action<SchemaBuilder>> _lateBuild = new List<Action<SchemaBuilder>>();

        private readonly Dictionary<string, INamedType> _types =
            new Dictionary<string, INamedType>();

        private ConnectionBuilder _connections;

        public SchemaBuilder()
        {
            _connections = new ConnectionBuilder(this);

            foreach (var scalarType in ScalarType.Standard)
                Include(scalarType);

            IncludeDirective(type.DirectiveType.Include);
            IncludeDirective(type.DirectiveType.Skip);
        }

        public SchemaBuilder(ISchema from) : this()
        {
            foreach (var namedType in from.QueryTypes<INamedType>())
            {
                if (IsPredefinedType<INamedType>(namedType.Name, out _))
                    continue;

                switch (namedType)
                {
                    case ObjectType objectType:
                        Include(objectType);
                        _connections.IncludeFields(objectType, from.GetFields(objectType.Name));
                        break;
                    case InterfaceType interfaceType:
                        Include(interfaceType);
                        _connections.IncludeFields(interfaceType, from.GetFields(interfaceType.Name));
                        break;
                    case InputObjectType inputType:
                        Include(inputType);
                        _connections.IncludeInputFields(inputType, from.GetInputFields(inputType.Name));
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

        public SchemaBuilder Connections(Action<ConnectionBuilder> connections)
        {
            if (connections == null) throw new ArgumentNullException(nameof(connections));
            connections(_connections);

            return this;
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

        public SchemaBuilder Enum(string name,
            out EnumType enumType,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params (string value, string description, IEnumerable<DirectiveInstance> directives, string
                deprecationReason)[] values)
        {
            enumType = new EnumType(name, new EnumValues(values), new Meta(description, directives: directives));
            _types.Add(name, enumType);
            return this;
        }

        public ISchema Build()
        {
            foreach (var lateBuildAction in _lateBuild) 
                lateBuildAction(this);

            var (fields, inputFields) = _connections.Build();
            return new SchemaGraph(
                _types,
                fields,
                inputFields,
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
            string description = null,
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

        public SchemaBuilder Scalar(string name, out ScalarType scalarType, IValueConverter converter,
            string description = null, IEnumerable<DirectiveInstance> directives = null)
        {
            scalarType = new ScalarType(name, converter, new Meta(description, null, directives));
            _types.Add(name, scalarType);
            return this;
        }
    }
}