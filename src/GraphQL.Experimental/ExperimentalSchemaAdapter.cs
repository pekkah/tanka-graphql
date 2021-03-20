using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.TypeSystem.ValueSerialization;
using Tanka.GraphQL.ValueResolution;
using Argument = Tanka.GraphQL.TypeSystem.Argument;

namespace Tanka.GraphQL.Experimental
{
    public class ExperimentalSchemaAdapter : ISchema
    {
        private readonly IResolverMap _resolvers;
        private readonly IReadOnlyDictionary<string, IValueConverter> _converters;
        private readonly ExecutableSchema _schema;

        public ExperimentalSchemaAdapter(
            ExecutableSchema schema,
            IResolverMap resolvers,
            IReadOnlyDictionary<string, IValueConverter> converters)
        {
            _schema = schema;
            _resolvers = resolvers;
            _converters = converters;
        }

        public IEnumerable<DirectiveInstance> Directives { get; } = Array.Empty<DirectiveInstance>();

        public DirectiveInstance? GetDirective(string name)
        {
            return null;
        }

        public bool HasDirective(string name)
        {
            return false;
        }

        public ObjectType? Subscription => Adapt(_schema.Subscription);

        public ObjectType Query => Adapt(_schema.Query)!;

        public ObjectType? Mutation => Adapt(_schema.Mutation);

        public INamedType GetNamedType(string name)
        {
            var node = _schema.GetNamedType<TypeDefinition>(name);

            return Adapt(node)!;
        }

        public IField GetField(string type, string name)
        {
            var node = _schema.GetNamedType<ObjectDefinition>(type);
            var fieldNode = node.Fields
                ?.Single(f => f.Name.Value == name)!;

            return new Field(
                Adapt(fieldNode.Type),
                Adapt(fieldNode.Arguments),
                fieldNode.Description!,
                null,
                fieldNode.Directives?.Select(Adapt)!
            );
        }

        private Args? Adapt(ArgumentsDefinition? nodes)
        {
            if (nodes == null)
                return null;

            var args = nodes
                .ToDictionary(
                    a => a.Name.Value, 
                    Adapt);

            return new Args(args);
        }

        private Argument Adapt(InputValueDefinition node)
        {
            return new Argument(
                Adapt(node.Type),
                null,
                node.Description!,
                node.Directives?.Select(Adapt)!
            );
        }

        public IEnumerable<KeyValuePair<string, IField>> GetFields(string type)
        {
            throw new NotImplementedException(nameof(GetFields));
        }

        public IQueryable<T> QueryTypes<T>(Predicate<T>? filter = null) where T : INamedType
        {
            throw new NotImplementedException(nameof(QueryTypes));
        }

        public DirectiveType GetDirectiveType(string name)
        {
            throw new NotImplementedException(nameof(GetDirectiveType));
        }

        public IQueryable<DirectiveType> QueryDirectiveTypes(Predicate<DirectiveType> filter = null)
        {
            throw new NotImplementedException(nameof(QueryDirectiveTypes));
        }

        public IEnumerable<KeyValuePair<string, InputObjectField>> GetInputFields(string type)
        {
            throw new NotImplementedException(nameof(GetInputFields));
        }

        public InputObjectField GetInputField(string type, string name)
        {
            var node = _schema.GetNamedType<InputObjectDefinition>(type);
            var fieldNode = node.Fields
                ?.Single(f => f.Name.Value == name)!;

            return new InputObjectField(
                Adapt(fieldNode.Type),
                fieldNode.Description!,
                null,
                fieldNode.Directives?.Select(Adapt)!
            );
        }

        private IType Adapt(TypeBase type)
        {
            return type switch
            {
                ListType listType => Adapt(listType),
                NamedType namedType => Adapt(namedType),
                NonNullType nonNullType => Adapt(nonNullType),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private NonNull Adapt(NonNullType nonNullType)
        {
            return new NonNull(Adapt(nonNullType.OfType));
        }

        private INamedType Adapt(NamedType namedType)
        {
            return GetNamedType(namedType.Name);
        }

        private List Adapt(ListType listType)
        {
            return new List(Adapt(listType.OfType));
        }

        public IEnumerable<ObjectType> GetPossibleTypes(IAbstractType abstractType)
        {
            throw new NotImplementedException(nameof(GetPossibleTypes));
        }

        public Resolver GetResolver(string type, string fieldName)
        {
            return _resolvers.GetResolver(type, fieldName);
        }

        public Subscriber GetSubscriber(string type, string fieldName)
        {
            throw new NotImplementedException(nameof(GetSubscriber));
        }

        public IValueConverter GetValueConverter(string type)
        {
            return _converters[type];
        }

        private INamedType? Adapt(TypeDefinition? node)
        {
            return node switch
            {
                null => null,
                EnumDefinition enumDefinition => Adapt(enumDefinition),
                InputObjectDefinition inputObjectDefinition => Adapt(inputObjectDefinition),
                InterfaceDefinition interfaceDefinition => Adapt(interfaceDefinition),
                ObjectDefinition objectDefinition => Adapt(objectDefinition),
                ScalarDefinition scalarDefinition => Adapt(scalarDefinition),
                UnionDefinition unionDefinition => Adapt(unionDefinition),
                _ => throw new ArgumentOutOfRangeException(nameof(node))
            };
        }

        private INamedType? Adapt(UnionDefinition node)
        {
            return new UnionType(
                node.Name,
                Array.Empty<ObjectType>(),
                node.Description!,
                node.Directives?.Select(Adapt)!
            );
        }

        private INamedType? Adapt(ScalarDefinition node)
        {
            return new ScalarType(
                node.Name,
                node.Description!,
                node.Directives?.Select(Adapt)!
            );
        }

        private INamedType? Adapt(InterfaceDefinition node)
        {
            return new InterfaceType(
                node.Name,
                node.Description!,
                node.Directives?.Select(Adapt)!
            );
        }

        private INamedType? Adapt(InputObjectDefinition node)
        {
            return new InputObjectType(
                node.Name,
                node.Description!,
                node.Directives?.Select(Adapt)!
            );
        }

        private DirectiveInstance Adapt(Directive node)
        {
            throw new NotImplementedException("Adapt(Directive node)");
        }

        private ObjectType? Adapt(ObjectDefinition? node)
        {
            if (node == null)
                return null;

            return new ObjectType(
                node.Name,
                node.Description
            );
        }
    }
}