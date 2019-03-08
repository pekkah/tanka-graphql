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
            {
                Union(definition);
            }

            foreach (var definition in definitions.OfType<GraphQLTypeExtensionDefinition>())
                Extend(definition);

            return _builder;
        }

        protected ScalarType Scalar(GraphQLScalarTypeDefinition definition)
        {
            _builder.GetScalar(definition.Name.Value, out var scalar);

            if (scalar == null)
                throw new GraphQLError(
                    $"Scalar type '{definition.Name.Value}' not known. Add it to the context before parsing.",
                    definition);

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
            foreach (var definition in definitions)
            {
                var type = InputType(definition.Type);
                object defaultValue = null;

                /* schema is required???
                try
                {
                    defaultValue = Values.CoerceValue(definition.DefaultValue, type);
                }
                catch (Exception)
                {
                    defaultValue = null;
                }
                */

                yield return (definition.Name.Value, type, defaultValue, default);
            }
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
                // so we have InputObject, Enum or Scalar
                case GraphQLScalarTypeDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case GraphQLEnumTypeDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case GraphQLInputObjectTypeDefinition inputObjectTypeDefinition:
                    return InputObject(inputObjectTypeDefinition);
            }

            //todo: we should not come here ever
            return null;
        }

        protected IType OutputType(GraphQLType typeDefinition)
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
                .OutputType(typeName);

            if (definition == null)
                throw new InvalidOperationException(
                    $"Could not find a input type definition '{typeName}'.");

            switch (definition)
            {
                // so we have InputObject, Enum or Scalar
                case GraphQLScalarTypeDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case GraphQLEnumTypeDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case GraphQLInputObjectTypeDefinition inputObjectTypeDefinition:
                    return InputObject(inputObjectTypeDefinition);
                case GraphQLObjectTypeDefinition objectType:
                    return Object(objectType);
                case GraphQLInterfaceTypeDefinition interfaceType:
                    return Interface(interfaceType);
                case GraphQLUnionTypeDefinition unionType:
                    return Union(unionType);
            }

            //todo: we should not come here ever
            return null;
        }

        protected EnumType Enum(GraphQLEnumTypeDefinition definition)
        {
            var values = definition.Values.Select(value =>
                (value.Name.Value, string.Empty, Directives(definition.Directives), string.Empty));

            var directives = Directives(definition.Directives);

            _builder.Enum(definition.Name.Value, out var enumType,
                directives: directives,
                description:null,
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

            _builder.TryGetType<DirectiveType>(name, out var directiveType);

            if (directiveType == null)
                throw new GraphQLError(
                    $"Could not find DirectiveType with name '{name}'");

            var arguments = new Args();
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
                    //todo: this needs it's own type
                    arguments.Add(argument.Key, new Argument(type, defaultValue));
                    continue;
                }

                if (type is NonNull
                    && (!hasValue || value == null))
                    throw new ValueCoercionException(
                        $"Argument {argument.Key} type is non-nullable but value is null or not set",
                        null,
                        directiveType);

                if (hasValue)
                    arguments.Add(argument.Key,
                        value == null
                            ? new Argument(type, defaultValue)
                            : new Argument(type, default /* Coerce value*/)); //todo: coerce default value
            }

            yield return directiveType.CreateInstance(arguments);
        }

        protected InputObjectType InputObject(GraphQLInputObjectTypeDefinition definition)
        {
            _builder.InputObject(definition.Name.Value, out var inputObject);
            _builder.LateBuild(builder =>
            {
                builder.Connections(connect =>
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

                /* coercion requires schema
                try
                {
                    defaultValue = Values.CoerceValue(definition.DefaultValue, type);
                }
                catch (ValueCoercionException)
                {
                    defaultValue = null;
                }
                */

                var directives = Directives(definition.Directives);

                switch (type)
                {
                    case ScalarType scalarType:
                        fields[definition.Name.Value] = new InputObjectField(
                            scalarType,
                            new Meta(directives: directives),
                            defaultValue);
                        break;
                    case EnumType enumType:
                        fields[definition.Name.Value] = new InputObjectField(
                            enumType,
                            new Meta(directives: directives),
                            defaultValue);
                        break;
                    case InputObjectType inputObjectType:
                        fields[definition.Name.Value] = new InputObjectField(
                            inputObjectType,
                            new Meta(directives: directives),
                            defaultValue);
                        break;
                    case NonNull nonNull:
                        fields[definition.Name.Value] = new InputObjectField(
                            nonNull,
                            new Meta(directives: directives),
                            defaultValue);
                        break;
                    case List list:
                        fields[definition.Name.Value] = new InputObjectField(
                            list,
                            new Meta(directives: directives),
                            defaultValue);
                        break;
                }
            }

            return fields;
        }

        protected InterfaceType Interface(GraphQLInterfaceTypeDefinition definition)
        {
            var directives = Directives(definition.Directives);

            _builder.Interface(definition.Name.Value, out var interfaceType,
                directives: directives);

            _builder.LateBuild(_ => { Fields(interfaceType, definition.Fields); });

            return interfaceType;
        }

        protected ObjectType Object(GraphQLObjectTypeDefinition definition)
        {
            var directives = Directives(definition.Directives);
            var interfaces = Interfaces(definition.Interfaces);

            _builder.Object(definition.Name.Value, out var objectType,
                interfaces: interfaces,
                directives: directives);

            _builder.LateBuild(_ => { Fields(objectType, definition.Fields); });

            return objectType;
        }

        protected void Extend(GraphQLTypeExtensionDefinition definition)
        {
            _builder.LateBuild(_ =>
            {
                if (!_builder.TryGetType<ObjectType>(definition.Definition.Name.Value, out var type))
                    throw new InvalidOperationException(
                        $"Cannot extend type '{definition.Definition.Name}'. Type to extend not found.");

                Fields(type, definition.Definition.Fields);
            });
        }

        private IType Union(GraphQLUnionTypeDefinition definition)
        {
            var possibleTypes = new List<ObjectType>();
            foreach (var astType in definition.Types)
            {
                var type = (ObjectType) OutputType(astType);
                possibleTypes.Add(type);
            }

            var directives = Directives(definition.Directives);
            _builder.Union(definition.Name.Value, out var unionType, default, directives, possibleTypes.ToArray());
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