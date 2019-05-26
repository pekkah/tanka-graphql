using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.error;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.sdl
{
    public class SdlReader
    {
        private readonly List<Action<SchemaBuilder>> _afterTypeDefinitions = new List<Action<SchemaBuilder>>();
        private readonly SchemaBuilder _builder;
        private readonly GraphQLDocument _document;


        public SdlReader(GraphQLDocument document, SchemaBuilder builder = null)
        {
            _document = document;
            _builder = builder ?? new SchemaBuilder();
        }

        public SchemaBuilder Read()
        {
            var definitions = _document.Definitions.ToList();

            foreach (var definition in definitions.OfType<GraphQLScalarTypeDefinition>())
                Scalar(definition);

            foreach (var directiveDefinition in definitions.OfType<GraphQLDirectiveDefinition>())
                DirectiveType(directiveDefinition);

            foreach (var definition in definitions.OfType<GraphQLInputObjectTypeDefinition>())
                InputObject(definition);

            foreach (var definition in definitions.OfType<GraphQLEnumTypeDefinition>())
                Enum(definition);

            foreach (var definition in definitions.OfType<GraphQLInterfaceTypeDefinition>())
                Interface(definition);

            foreach (var definition in definitions.OfType<GraphQLObjectTypeDefinition>())
                Object(definition);

            foreach (var definition in definitions.OfType<GraphQLUnionTypeDefinition>())
                Union(definition);

            foreach (var definition in definitions.OfType<GraphQLTypeExtensionDefinition>())
                Extend(definition);

            foreach (var action in _afterTypeDefinitions) action(_builder);

            return _builder;
        }

        protected ScalarType Scalar(GraphQLScalarTypeDefinition definition)
        {
            _builder.GetScalar(definition.Name.Value, out var scalar);
            return scalar;
        }

        protected DirectiveType DirectiveType(GraphQLDirectiveDefinition definition)
        {
            var locations = definition.Locations
                .Select(location =>
                    (DirectiveLocation) System.Enum.Parse(typeof(DirectiveLocation), location.Value))
                .ToList();

            var args = Args(definition.Arguments);

            _builder.DirectiveType(
                definition.Name.Value,
                out var directiveType,
                locations,
                null,
                args.ToArray());

            return directiveType;
        }

        protected IEnumerable<(string Name, IType Type, object DefaultValue, string Description)> Args(
            IEnumerable<GraphQLInputValueDefinition> definitions)
        {
            var args = new List<(string Name, IType Type, object DefaultValue, string Description)>();
            foreach (var definition in definitions)
            {
                var type = InputType(definition.Type);
                object defaultValue = null;

                try
                {
                    _builder.Connections(connections =>
                    {
                        defaultValue = Values.CoerceValue(
                            connections.GetInputFields,
                            definition.DefaultValue,
                            type);
                    });
                }
                catch (Exception)
                {
                    defaultValue = null;
                }

                args.Add((definition.Name.Value, type, defaultValue, default));
            }

            return args;
        }

        protected IType InputType(GraphQLType typeDefinition)
        {
            if (typeDefinition.Kind == ASTNodeKind.NonNullType)
            {
                var innerType = InputType(((GraphQLNonNullType) typeDefinition).Type);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeDefinition.Kind == ASTNodeKind.ListType)
            {
                var innerType = InputType(((GraphQLListType) typeDefinition).Type);
                return innerType != null ? new List(innerType) : null;
            }

            var namedTypeDefinition = (GraphQLNamedType) typeDefinition;
            var typeName = namedTypeDefinition.Name.Value;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to get the
            // definition and build it
            var definition = _document.Definitions
                .OfType<GraphQLTypeDefinition>()
                .InputType(typeName);

            if (definition == null)
                throw new InvalidOperationException(
                    $"Could not find a input type definition '{typeName}'.");

            switch (definition)
            {
                case GraphQLScalarTypeDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case GraphQLEnumTypeDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case GraphQLInputObjectTypeDefinition inputObjectTypeDefinition:
                    return InputObject(inputObjectTypeDefinition);
            }

            // we should not come here ever
            return null;
        }

        protected IType OutputType(GraphQLType typeDefinition)
        {
            if (typeDefinition.Kind == ASTNodeKind.NonNullType)
            {
                var innerType = OutputType(((GraphQLNonNullType) typeDefinition).Type);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeDefinition.Kind == ASTNodeKind.ListType)
            {
                var innerType = OutputType(((GraphQLListType) typeDefinition).Type);
                return innerType != null ? new List(innerType) : null;
            }

            var namedTypeDefinition = (GraphQLNamedType) typeDefinition;
            var typeName = namedTypeDefinition.Name.Value;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to get the
            // definition and build it
            var definition = _document.Definitions
                .OfType<GraphQLTypeDefinition>()
                .OutputType(typeName);

            if (definition == null)
                throw new InvalidOperationException(
                    $"Could not find a input type definition '{typeName}'.");

            switch (definition)
            {
                case GraphQLScalarTypeDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case GraphQLEnumTypeDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case GraphQLObjectTypeDefinition objectType:
                    return Object(objectType);
                case GraphQLInterfaceTypeDefinition interfaceType:
                    return Interface(interfaceType);
                case GraphQLUnionTypeDefinition unionType:
                    return Union(unionType);
            }

            return null;
        }

        protected EnumType Enum(GraphQLEnumTypeDefinition definition)
        {
            var values = definition.Values.Select(value =>
                (value.Name.Value, string.Empty, Directives(definition.Directives), string.Empty));

            var directives = Directives(definition.Directives);

            _builder.Enum(definition.Name.Value, out var enumType,
                directives: directives,
                description: null,
                values: values.ToArray());

            return enumType;
        }

        protected IEnumerable<DirectiveInstance> Directives(
            IEnumerable<GraphQLDirective> directives)
        {
            foreach (var directive in directives)
            foreach (var directiveInstance in DirectiveInstance(directive))
                yield return directiveInstance;
        }

        protected IEnumerable<DirectiveInstance> DirectiveInstance(
            GraphQLDirective directiveDefinition)
        {
            var name = directiveDefinition.Name.Value;

            _builder.TryGetDirective(name, out var directiveType);

            if (directiveType == null)
                throw new GraphQLError(
                    $"Could not find DirectiveType with name '{name}'");

            var arguments = new Dictionary<string, object>();
            foreach (var argument in directiveType.Arguments)
            {
                var type = argument.Value.Type;
                var defaultValue = argument.Value.DefaultValue;

                var definition = directiveDefinition.Arguments
                    .SingleOrDefault(a => a.Name.Value == argument.Key);

                var hasValue = definition != null;
                var value = definition?.Value;

                if (!hasValue && defaultValue != null)
                {
                    arguments.Add(argument.Key, defaultValue);
                    continue;
                }

                if (type is NonNull
                    && (!hasValue || value == null))
                    throw new ValueCoercionException(
                        $"Argument {argument.Key} type is non-nullable but value is null or not set",
                        null,
                        directiveType);

                _builder.Connections(connect =>
                {
                    if (hasValue)
                        arguments.Add(argument.Key,
                                value == null
                                    ? defaultValue
                                    : Values.CoerceValue(
                                        connect.GetInputFields,
                                        value,
                                        type));
                });
            }

            yield return directiveType.CreateInstance(arguments);
        }

        protected InputObjectType InputObject(GraphQLInputObjectTypeDefinition definition)
        {
            if (_builder.TryGetType<InputObjectType>(definition.Name.Value, out var inputObject)) return inputObject;

            _builder.InputObject(definition.Name.Value, out inputObject);
            _builder.Connections(connect =>
            {
                var fields = InputValues(definition.Fields);
                foreach (var inputField in fields)
                    connect.InputField(
                        inputObject,
                        inputField.Key,
                        inputField.Value.Type,
                        inputField.Value.DefaultValue,
                        inputField.Value.Description,
                        inputField.Value.Directives);
            });

            return inputObject;
        }

        protected InputFields InputValues(IEnumerable<GraphQLInputValueDefinition> definitions)
        {
            var fields = new InputFields();

            foreach (var definition in definitions)
            {
                var type = InputType(definition.Type);

                if (!TypeIs.IsInputType(type))
                    throw new GraphQLError(
                        "Type of input value definition is not valid input value type. " +
                        $"Definition: '{definition.Name.Value}' Type: {definition.Type.Kind}",
                        definition);

                object defaultValue = default;

                _builder.Connections(connect =>
                {
                    try
                    {
                        defaultValue = Values.CoerceValue(
                            connect.GetInputFields,
                            definition.DefaultValue,
                            type);
                    }
                    catch (ValueCoercionException)
                    {
                        defaultValue = null;
                    }
                });

                var directives = Directives(definition.Directives);

                switch (type)
                {
                    case ScalarType scalarType:
                        fields[definition.Name.Value] = new InputObjectField(
                            scalarType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case EnumType enumType:
                        fields[definition.Name.Value] = new InputObjectField(
                            enumType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case InputObjectType inputObjectType:
                        fields[definition.Name.Value] = new InputObjectField(
                            inputObjectType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case NonNull nonNull:
                        fields[definition.Name.Value] = new InputObjectField(
                            nonNull,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case List list:
                        fields[definition.Name.Value] = new InputObjectField(
                            list,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                }
            }

            return fields;
        }

        protected InterfaceType Interface(GraphQLInterfaceTypeDefinition definition)
        {
            if (_builder.TryGetType<InterfaceType>(definition.Name.Value, out var interfaceType)) return interfaceType;

            var directives = Directives(definition.Directives);

            _builder.Interface(definition.Name.Value, out interfaceType,
                directives: directives);

            AfterTypeDefinitions(_ => { Fields(interfaceType, definition.Fields); });

            return interfaceType;
        }

        protected ObjectType Object(GraphQLObjectTypeDefinition definition)
        {
            if (_builder.TryGetType<ObjectType>(definition.Name.Value, out var objectType)) return objectType;

            var directives = Directives(definition.Directives);
            var interfaces = Interfaces(definition.Interfaces);

            _builder.Object(definition.Name.Value, out objectType,
                interfaces: interfaces,
                directives: directives);

            AfterTypeDefinitions(_ => { Fields(objectType, definition.Fields); });

            return objectType;
        }

        protected void Extend(GraphQLTypeExtensionDefinition definition)
        {
            AfterTypeDefinitions(_ =>
            {
                if (!_builder.TryGetType<ObjectType>(definition.Definition.Name.Value, out var type))
                    throw new InvalidOperationException(
                        $"Cannot extend type '{definition.Definition.Name}'. Type to extend not found.");

                Fields(type, definition.Definition.Fields);
            });
        }

        private void AfterTypeDefinitions(Action<SchemaBuilder> action)
        {
            _afterTypeDefinitions.Add(action);
        }

        private UnionType Union(GraphQLUnionTypeDefinition definition)
        {
            if (_builder.TryGetType<UnionType>(definition.Name.Value, out var unionType)) return unionType;

            var possibleTypes = new List<ObjectType>();
            foreach (var astType in definition.Types)
            {
                var type = (ObjectType) OutputType(astType);
                possibleTypes.Add(type);
            }

            var directives = Directives(definition.Directives);
            _builder.Union(definition.Name.Value, out unionType, default, directives, possibleTypes.ToArray());
            return unionType;
        }

        private IEnumerable<InterfaceType> Interfaces(IEnumerable<GraphQLNamedType> definitions)
        {
            var interfaces = new List<InterfaceType>();

            foreach (var namedType in definitions)
            {
                var type = OutputType(namedType) as InterfaceType;

                if (type == null)
                    continue;

                interfaces.Add(type);
            }

            return interfaces;
        }

        private void Fields(ComplexType owner, IEnumerable<GraphQLFieldDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                var type = OutputType(definition.Type);
                var name = definition.Name.Value;
                var args = Args(definition.Arguments);
                var directives = Directives(definition.Directives);

                _builder.Connections(connect => connect
                    .Field(owner, name, type, default, null, null, directives, args.ToArray()));
            }
        }
    }
}