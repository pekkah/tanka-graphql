using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class IntrospectionSchemaReader
    {
        private readonly SchemaBuilder _builder;

        private readonly List<Action> _delayedActions = new List<Action>();

        public IntrospectionSchemaReader(SchemaBuilder builder)
        {
            _builder = builder;
        }

        public void Read(IntrospectionResult result)
        {
            var types = result.Schema.Types;

            foreach (var type in types.Where(t => t.Kind == __TypeKind.SCALAR))
                Scalar(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.INPUT_OBJECT))
                InputObject(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.ENUM))
                Enum(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.INTERFACE))
                Interface(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.OBJECT))
                Object(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.OBJECT))
                Union(type);

            foreach (var action in _delayedActions)
                action();
        }

        protected IType InputType(__Type typeDefinition)
        {
            if (typeDefinition.Kind == __TypeKind.NON_NULL)
            {
                var innerType = InputType(typeDefinition.OfType);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeDefinition.Kind == __TypeKind.LIST)
            {
                var innerType = InputType(typeDefinition.OfType);
                return innerType != null ? new List(innerType) : null;
            }

            var typeName = typeDefinition.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to build it
            switch (typeDefinition.Kind)
            {
                case __TypeKind.SCALAR:
                    return Scalar(typeDefinition);
                case __TypeKind.ENUM:
                    return Enum(typeDefinition);
                case __TypeKind.INPUT_OBJECT:
                    return InputObject(typeDefinition);
            }

            // we should not come here ever
            return null;
        }

        protected IType OutputType(__Type typeDefinition)
        {
            if (typeDefinition.Kind == __TypeKind.NON_NULL)
            {
                var innerType = OutputType(typeDefinition.OfType);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeDefinition.Kind == __TypeKind.LIST)
            {
                var innerType = OutputType(typeDefinition.OfType);
                return innerType != null ? new List(innerType) : null;
            }

            var typeName = typeDefinition.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // type is not known so we need to build it
            switch (typeDefinition.Kind)
            {
                case __TypeKind.SCALAR:
                    return Scalar(typeDefinition);
                case __TypeKind.ENUM:
                    return Enum(typeDefinition);
                case __TypeKind.OBJECT:
                    return Object(typeDefinition);
                case __TypeKind.INTERFACE:
                    return Interface(typeDefinition);
                case __TypeKind.UNION:
                    return Union(typeDefinition);
            }

            return null;
        }

        private IType Scalar(__Type type)
        {
            // throws if scalar not found
            _builder.GetScalar(type.Name, out var scalar);
            return scalar;
        }

        private EnumType Enum(__Type type)
        {
            var values =
                type.EnumValues
                    .Select(v => (v.Name, v.Description, default(IEnumerable<DirectiveInstance>), v.DeprecationReason))
                    .ToArray();

            _builder.Enum(
                type.Name,
                out var enumType,
                type.Description,
                null,
                values);

            return enumType;
        }

        private InputObjectType InputObject(__Type type)
        {
            _builder.InputObject(type.Name, out var owner, type.Description, null);
            _builder.Connections(connect =>
            {
                foreach (var field in type.InputFields)
                    connect.InputField(
                        owner,
                        field.Name,
                        InputType(field.Type),
                        field.DefaultValue,
                        field.Description);
            });

            return owner;
        }

        private InterfaceType Interface(__Type type)
        {
            _builder.Interface(type.Name, out var owner, type.Description, null);
            _delayedActions.Add(() =>
            {
                _builder.Connections(connect =>
                {
                    foreach (var field in type.Fields)
                    {
                        (string Name, IType Type, object DefaultValue, string Description)[] args = field.Args
                            .Select(arg => (arg.Name, InputType(arg.Type), (object) arg.DefaultValue, arg.Description))
                            .ToArray();

                        connect.Field(owner, field.Name, OutputType(field.Type), field.Description, args: args);
                    }
                });
            });

            return owner;
        }

        private ObjectType Object(__Type type)
        {
            var interfaces = type.Interfaces.Select(Interface)
                .ToList();

            _builder.Object(
                type.Name,
                out var owner,
                type.Description,
                interfaces);

            _delayedActions.Add(() =>
            {
                _builder.Connections(connect =>
                {
                    foreach (var field in type.Fields)
                    {
                        (string Name, IType Type, object DefaultValue, string Description)[] args = field.Args
                            .Select(arg => (arg.Name, InputType(arg.Type), (object) arg.DefaultValue, arg.Description))
                            .ToArray();

                        connect.Field(owner, field.Name, OutputType(field.Type), field.Description, args: args);
                    }
                });
            });

            return owner;
        }

        private UnionType Union(__Type type)
        {
            var possibleTypes = type.PossibleTypes
                .Select(possibleType => (ObjectType) OutputType(possibleType))
                .ToArray();

            _builder.Union(
                type.Name,
                out var unionType,
                type.Description,
                possibleTypes: possibleTypes);

            return unionType;
        }
    }
}