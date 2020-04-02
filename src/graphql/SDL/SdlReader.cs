using System;
using System.Collections.Generic;
using System.Linq;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.SDL
{
    public class SdlReader
    {
        private readonly List<Action<SchemaBuilder>> _afterTypeDefinitions = new List<Action<SchemaBuilder>>();
        private readonly SchemaBuilder _builder;
        private readonly TypeSystemDocument _document;


        public SdlReader(TypeSystemDocument document, SchemaBuilder builder = null)
        {
            _document = document;
            _builder = builder ?? new SchemaBuilder();
        }

        public SchemaBuilder Read()
        {
            var definitions = _document.TypeDefinitions ?? Array.Empty<TypeDefinition>();

            foreach (var definition in definitions.OfType<ScalarDefinition>())
                Scalar(definition);

            if (_document.DirectiveDefinitions != null)
                foreach (var directiveDefinition in _document.DirectiveDefinitions)
                    DirectiveType(directiveDefinition);

            foreach (var definition in definitions.OfType<InputObjectDefinition>())
                InputObject(definition);

            foreach (var definition in definitions.OfType<EnumDefinition>())
                Enum(definition);

            foreach (var definition in definitions.OfType<InterfaceDefinition>())
                Interface(definition);

            foreach (var definition in definitions.OfType<ObjectDefinition>())
                Object(definition);

            foreach (var definition in definitions.OfType<UnionDefinition>())
                Union(definition);

            if (_document.TypeExtensions != null)
                foreach (var definition in _document.TypeExtensions)
                 Extend(definition);

            foreach (var action in _afterTypeDefinitions) action(_builder);

            var schemaDefinition = _document
                .SchemaDefinitions
                ?.SingleOrDefault();

            if (schemaDefinition?.Directives != null)
            {
                var directives = Directives(schemaDefinition.Directives);
                _builder.Schema(
                    schemaDefinition.Description,
                    directives);
            }

            return _builder;
        }

        protected ScalarType Scalar(ScalarDefinition definition)
        {
            _builder.Scalar(
                definition.Name,
                out var scalar,
                definition.Description,
                Directives(definition.Directives));

            return scalar;
        }

        protected DirectiveType DirectiveType(DirectiveDefinition definition)
        {
            var locations = definition
                .DirectiveLocations
                .Select(location =>
                    (DirectiveLocation) System.Enum.Parse(typeof(DirectiveLocation), location))
                .ToList();

            var args = Args(definition.Arguments).ToList();

            _builder.DirectiveType(
                definition.Name,
                out var directiveType,
                locations,
                null,
                argsBuilder => args.ForEach(a => argsBuilder.Arg(a.Name, a.Type, a.DefaultValue, a.Description)));

            return directiveType;
        }

        protected IEnumerable<(string Name, IType Type, object DefaultValue, string Description)> Args(
            IEnumerable<InputValueDefinition>? definitions)
        {
            var args = new List<(string Name, IType Type, object DefaultValue, string Description)>();

            if (definitions == null)
                return args;

            foreach (var definition in definitions)
            {
                var type = InputType(definition.Type);
                object? defaultValue = null;

                try
                {
                    _builder.Connections(connections =>
                    {
                        defaultValue = Values.CoerceValue(
                            connections.GetInputFields,
                            _builder.GetValueConverter,
                            definition.DefaultValue?.Value,
                            type);
                    });
                }
                catch (Exception)
                {
                    defaultValue = null;
                }

                args.Add((definition.Name, type, defaultValue, default));
            }

            return args;
        }

        protected IType InputType(TypeBase type)
        {
            if (type.Kind == NodeKind.NonNullType)
            {
                var innerType = InputType(((NonNullType) type).OfType);
                return new NonNull(innerType);
            }

            if (type.Kind == NodeKind.ListType)
            {
                var innerType = InputType(((ListType) type).OfType);
                return new List(innerType);
            }

            var namedTypeDefinition = (NamedType) type;
            var typeName = namedTypeDefinition.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to get the
            // definition and build it
            var definition = _document.TypeDefinitions!
                .InputType(typeName);

            if (definition == null)
                throw new InvalidOperationException(
                    $"Could not find a input type definition '{typeName}'.");

            switch (definition)
            {
                case ScalarDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case EnumDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case InputObjectDefinition inputObjectTypeDefinition:
                    return InputObject(inputObjectTypeDefinition);
            }

            // we should not come here ever
            return null;
        }

        protected IType OutputType(TypeBase type)
        {
            if (type.Kind == NodeKind.NonNullType)
            {
                var innerType = OutputType(((NonNullType) type).OfType);
                return new NonNull(innerType);
            }

            if (type.Kind == NodeKind.ListType)
            {
                var innerType = OutputType(((ListType) type).OfType);
                return new List(innerType);
            }

            var namedTypeDefinition = (NamedType) type;
            var typeName = namedTypeDefinition.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to get the
            // definition and build it
            var definition = _document.TypeDefinitions!
                .OutputType(typeName);

            if (definition == null)
                throw new InvalidOperationException(
                    $"Could not find a output type definition '{typeName}'.");

            switch (definition)
            {
                case ScalarDefinition scalarTypeDefinition:
                    return Scalar(scalarTypeDefinition);
                case EnumDefinition enumTypeDefinition:
                    return Enum(enumTypeDefinition);
                case ObjectDefinition objectType:
                    return Object(objectType);
                case InterfaceDefinition interfaceType:
                    return Interface(interfaceType);
                case UnionDefinition unionType:
                    return Union(unionType);
            }

            return null;
        }

        protected EnumType Enum(EnumDefinition definition)
        {
            if (_builder.TryGetType<EnumType>(definition.Name, out var enumType)) 
                return enumType;
            
            var directives = Directives(definition.Directives);

            _builder.Enum(definition.Name, out enumType,
                directives: directives,
                description: null,
                values: values => 
                    definition.Values.ToList()
                        .ForEach(value => values.Value(
                            value.Value.Name,
                            string.Empty,
                            Directives(value.Directives),
                            null)));

            return enumType;
        }

        protected IEnumerable<DirectiveInstance> Directives(
            IEnumerable<Directive>? directives)
        {
            if (directives == null)
                yield break;

            foreach (var directive in directives)
            foreach (var directiveInstance in DirectiveInstance(directive))
                yield return directiveInstance;
        }

        protected IEnumerable<DirectiveInstance> DirectiveInstance(
            Directive directiveDefinition)
        {
            var name = directiveDefinition.Name;

            _builder.TryGetDirective(name, out var directiveType);

            if (directiveType == null)
                throw new DocumentException(
                    $"Could not find DirectiveType with name '{name}'",
                    directiveDefinition);

            var arguments = new Dictionary<string, object?>();
            foreach (var argument in directiveType.Arguments)
            {
                var type = argument.Value.Type;
                var defaultValue = argument.Value.DefaultValue;

                var definition = directiveDefinition.Arguments?
                    .SingleOrDefault(a => a.Name == argument.Key);

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
                                        _builder.GetValueConverter,
                                        value,
                                        type));
                });
            }

            yield return directiveType.CreateInstance(arguments);
        }

        protected InputObjectType InputObject(InputObjectDefinition definition)
        {
            if (_builder.TryGetType<InputObjectType>(definition.Name, out var inputObject)) return inputObject;

            var directives = Directives(definition.Directives);
            _builder.InputObject(
                definition.Name, 
                out inputObject,
                description: definition.Description?.ToString(),
                directives: directives);
            
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

        protected InputFields InputValues(IEnumerable<InputValueDefinition>? definitions)
        {
            var fields = new InputFields();

            if (definitions == null)
                return fields;

            foreach (var definition in definitions)
            {
                var type = InputType(definition.Type);

                if (!TypeIs.IsInputType(type))
                    throw new DocumentException(
                        "Type of input value definition is not valid input value type. " +
                        $"Definition: '{definition.Name}' Type: {definition.Type.Kind}");

                object? defaultValue = default;

                _builder.Connections(connect =>
                {
                    try
                    {
                        defaultValue = Values.CoerceValue(
                            connect.GetInputFields,
                            _builder.GetValueConverter,
                            definition.DefaultValue,
                            type);
                    }
                    catch (Exception)
                    {
                        defaultValue = null;
                    }
                });

                var directives = Directives(definition.Directives);

                switch (type)
                {
                    case ScalarType scalarType:
                        fields[definition.Name] = new InputObjectField(
                            scalarType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case EnumType enumType:
                        fields[definition.Name] = new InputObjectField(
                            enumType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case InputObjectType inputObjectType:
                        fields[definition.Name] = new InputObjectField(
                            inputObjectType,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case NonNull nonNull:
                        fields[definition.Name] = new InputObjectField(
                            nonNull,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                    case List list:
                        fields[definition.Name] = new InputObjectField(
                            list,
                            string.Empty,
                            defaultValue,
                            directives);
                        break;
                }
            }

            return fields;
        }

        protected InterfaceType Interface(InterfaceDefinition definition)
        {
            if (_builder.TryGetType<InterfaceType>(definition.Name, out var interfaceType)) return interfaceType;

            var directives = Directives(definition.Directives);

            _builder.Interface(definition.Name, out interfaceType,
                directives: directives);

            AfterTypeDefinitions(_ => { Fields(interfaceType, definition.Fields); });

            return interfaceType;
        }

        protected ObjectType Object(ObjectDefinition definition)
        {
            if (_builder.TryGetType<ObjectType>(definition.Name, out var objectType)) return objectType;

            var directives = Directives(definition.Directives);
            var interfaces = Interfaces(definition.Interfaces);

            _builder.Object(definition.Name, out objectType,
                interfaces: interfaces,
                directives: directives);

            AfterTypeDefinitions(_ => { Fields(objectType, definition.Fields); });

            return objectType;
        }

        protected void Extend(TypeExtension extension)
        {
            AfterTypeDefinitions(_ =>
            {
                if (extension.ExtendedKind != NodeKind.ObjectDefinition)
                    throw new Exception($"Only object type extensions supported");
                
                if (!_builder.TryGetType<ObjectType>(extension.Name, out var type))
                    throw new InvalidOperationException(
                        $"Cannot extend type '{extension.Name}'. Type to extend not found.");

                var objectDefinition = (ObjectDefinition)extension.Definition;
                Fields(type, objectDefinition.Fields);
            });
        }

        private void AfterTypeDefinitions(Action<SchemaBuilder> action)
        {
            _afterTypeDefinitions.Add(action);
        }

        private UnionType Union(UnionDefinition definition)
        {
            if (_builder.TryGetType<UnionType>(definition.Name, out var unionType)) return unionType;

            var possibleTypes = new List<ObjectType>();
            if (definition.Members != null)
            {
                foreach (var astType in definition.Members)
                {
                    var type = (ObjectType) OutputType(astType);
                    possibleTypes.Add(type);
                }
            }

            var directives = Directives(definition.Directives);
            _builder.Union(definition.Name, out unionType, default, directives, possibleTypes.ToArray());
            return unionType;
        }

        private IEnumerable<InterfaceType> Interfaces(IEnumerable<NamedType> definitions)
        {
            var interfaces = new List<InterfaceType>();

            if (definitions == null)
                return Enumerable.Empty<InterfaceType>();

            foreach (var namedType in definitions)
            {
                if (!(OutputType(namedType) is InterfaceType type))
                    continue;

                interfaces.Add(type);
            }

            return interfaces;
        }

        private void Fields(ComplexType owner, IEnumerable<FieldDefinition>? definitions)
        {
            if (definitions == null)
                return;

            foreach (var definition in definitions)
            {
                var type = OutputType(definition.Type);
                var name = definition.Name;
                var args = Args(definition.Arguments).ToList();
                var directives = Directives(definition.Directives);

                _builder.Connections(connect => connect
                    .Field(
                        owner, 
                        name, 
                        type, 
                        default, 
                        null, 
                        null, 
                        directives, 
                        args: argsBuilder => args.ForEach(a => 
                            argsBuilder.Arg(a.Name, a.Type, a.DefaultValue, a.Description))));
            }
        }
    }
}