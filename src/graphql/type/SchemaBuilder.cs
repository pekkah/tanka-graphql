using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.resolvers;
using tanka.graphql.type.converters;

namespace tanka.graphql.type
{
    public class SchemaBuilder
    {
        private readonly ConnectionBuilder _connections;
        private readonly Dictionary<string, DirectiveType> _directives = new Dictionary<string, DirectiveType>();

        private readonly Dictionary<string, INamedType> _types =
            new Dictionary<string, INamedType>();

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
            Import(from);
        }

        public SchemaBuilder Import(ISchema from)
        {
            foreach (var namedType in from.QueryTypes<INamedType>())
            {
                if (TryGetType<INamedType>(namedType.Name, out _))
                    continue;

                switch (namedType)
                {
                    case ObjectType objectType:
                        Include(objectType);
                        var fields = from.GetFields(objectType.Name).ToList();
                        _connections.IncludeFields(objectType, fields);

                        foreach (var field in fields)
                        {
                            var resolver = from.GetResolver(objectType.Name, field.Key);

                            if (resolver != null)
                                _connections.GetOrAddResolver(objectType, field.Key)
                                    .Run(resolver);

                            var subscriber = from.GetSubscriber(objectType.Name, field.Key);

                            if (subscriber != null)
                                _connections.GetOrAddSubscriber(objectType, field.Key)
                                    .Run(subscriber);
                        }

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

            foreach (var directiveType in from.QueryDirectiveTypes())
            {
                if (_directives.ContainsKey(directiveType.Name))
                    continue;

                IncludeDirective(directiveType);
            }

            return this;
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
                description, 
                interfaces,
                directives: directives);

            Include(definition);
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
                description, 
                directives);

            Include(definition);
            return this;
        }

        public SchemaBuilder InputObject(
            string name,
            out InputObjectType definition,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null)
        {
            definition = new InputObjectType(name, description, directives);
            Include(definition);
            return this;
        }

        public SchemaBuilder Query(
            out ObjectType query,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Query", out query, description, interfaces);
            return this;
        }

        public SchemaBuilder Mutation(
            out ObjectType mutation,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Mutation", out mutation, description, interfaces);
            return this;
        }

        public SchemaBuilder Subscription(
            out ObjectType subscription,
            string description = null,
            IEnumerable<InterfaceType> interfaces = null)
        {
            Object("Subscription", out subscription, description, interfaces);
            return this;
        }

        public SchemaBuilder Enum(string name,
            out EnumType enumType,
            string description = null,
            IEnumerable<DirectiveInstance> directives = null,
            params (string value, string description, IEnumerable<DirectiveInstance> directives, string
                deprecationReason)[] values)
        {
            enumType = new EnumType(name, new EnumValues(values), description, directives);
            Include(enumType);
            return this;
        }

        public ISchema Build()
        {
            var (fields, inputFields, resolvers, subscribers) = _connections.Build();
            return new SchemaGraph(
                _types,
                fields,
                inputFields,
                _directives,
                BuildResolvers(resolvers),
                BuildSubscribers(subscribers));
        }

        public (ISchema Schema, object ValidationResult) BuildAndValidate()
        {
            var schema = Build();
            return (schema, new NotImplementedException("todo"));
        }

        public SchemaBuilder GetScalar(string name, out ScalarType scalarType)
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
            IncludeDirective(directiveType);
            return this;
        }

        public bool TryGetType<T>(string name, out T namedType)
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
            unionType = new UnionType(name, possibleTypes, description, directives);
            Include(unionType);
            return this;
        }

        public SchemaBuilder IncludeDirective(DirectiveType directiveType)
        {
            if (_directives.ContainsKey(directiveType.Name))
                throw new SchemaBuilderException(directiveType.Name,
                    $"Cannot include directive '{directiveType.Name}'. Directive already known.");

            _directives.Add(directiveType.Name, directiveType);
            return this;
        }

        public SchemaBuilder Include(INamedType type)
        {
            if (_types.ContainsKey(type.Name))
                throw new SchemaBuilderException(type.Name,
                    $"Cannot include type '{type.Name}'. Type already known.");

            _types.Add(type.Name, type);
            return this;
        }

        public SchemaBuilder Scalar(string name, out ScalarType scalarType, IValueConverter converter,
            string description = null, IEnumerable<DirectiveInstance> directives = null)
        {
            scalarType = new ScalarType(name, converter, description, directives);
            Include(scalarType);
            return this;
        }

        private Dictionary<string, Dictionary<string, Subscriber>> BuildSubscribers(
            Dictionary<string, Dictionary<string, SubscriberBuilder>> subscribers)
        {
            var result = new Dictionary<string, Dictionary<string, Subscriber>>();

            foreach (var type in subscribers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }

        private Dictionary<string, Dictionary<string, Resolver>> BuildResolvers(
            Dictionary<string, Dictionary<string, ResolverBuilder>> resolvers)
        {
            var result = new Dictionary<string, Dictionary<string, Resolver>>();

            foreach (var type in resolvers)
                result[type.Key] = type.Value.Select(f => (f.Key, f.Value.Build()))
                    .ToDictionary(f => f.Key, f => f.Item2);

            return result;
        }

        public IEnumerable<T> VisitTypes<T>() where T:INamedType
        {
            return _types.Values.OfType<T>();
        }

        public SchemaBuilder TryGetDirective(string name, out DirectiveType directiveType)
        {
            _directives.TryGetValue(name, out directiveType);
            return this;
        }
    }
}