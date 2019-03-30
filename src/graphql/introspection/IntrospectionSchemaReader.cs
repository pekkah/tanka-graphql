﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;

namespace tanka.graphql.introspection
{
    public class IntrospectionSchemaReader
    {
        private readonly SchemaBuilder _builder;

        private readonly ConcurrentQueue<Action> _delayedActions = new ConcurrentQueue<Action>();
        private readonly __Schema _schema;

        public IntrospectionSchemaReader(SchemaBuilder builder, IntrospectionResult result)
        {
            _builder = builder;
            _schema = result.Schema;
        }

        public void Read()
        {
            var types = _schema.Types
                .Distinct()
                .ToList();

            var queryTypeName = _schema.QueryType?.Name;
            var mutationTypeName = _schema.MutationType?.Name;
            var subscriptionTypeName = _schema.SubscriptionType?.Name;

            foreach (var type in types.Where(t => t.Kind == __TypeKind.SCALAR))
                Scalar(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.INPUT_OBJECT))
                InputObject(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.ENUM))
                Enum(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.INTERFACE))
                Interface(type);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.OBJECT))
                Object(
                    type,
                    type.Name == queryTypeName,
                    type.Name == mutationTypeName,
                    type.Name == subscriptionTypeName);

            foreach (var type in types.Where(t => t.Kind == __TypeKind.UNION))
                Union(type);

            while(_delayedActions.TryDequeue(out var action))
                action();
        }

        protected IType InputType(__Type typeReference)
        {
            if (typeReference.Kind == __TypeKind.NON_NULL)
            {
                var innerType = InputType(typeReference.OfType);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeReference.Kind == __TypeKind.LIST)
            {
                var innerType = InputType(typeReference.OfType);
                return innerType != null ? new List(innerType) : null;
            }

            var typeName = typeReference.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // get the actual type
            var typeDefinition = _schema.Types.Single(t => t.Name == typeReference.Name);

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

        protected IType OutputType(__Type typeReference)
        {
            if (typeReference.Kind == __TypeKind.NON_NULL)
            {
                var innerType = OutputType(typeReference.OfType);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeReference.Kind == __TypeKind.LIST)
            {
                var innerType = OutputType(typeReference.OfType);
                return innerType != null ? new List(innerType) : null;
            }

            var typeName = typeReference.Name;

            // is type already known by the builder?
            if (_builder.TryGetType<INamedType>(typeName, out var knownType))
                return knownType;

            // get the actual type
            var typeDefinition = _schema.Types.Single(t => t.Name == typeReference.Name);

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
            if (_builder.TryGetType<EnumType>(type.Name, out var enumType))
                return enumType;

            var values =
                type.EnumValues
                    .Select(v => (
                        v.Name,
                        v.Description,
                        default(IEnumerable<DirectiveInstance>),
                        v.DeprecationReason))
                    .ToArray();

            _builder.Enum(
                type.Name,
                out enumType,
                type.Description,
                null,
                values);

            return enumType;
        }

        private InputObjectType InputObject(__Type type)
        {
            if (_builder.TryGetType<InputObjectType>(type.Name, out var owner))
                return owner;

            _builder.InputObject(type.Name, out owner, type.Description, null);

            if (type.InputFields != null && type.InputFields.Any())
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
            if (_builder.TryGetType<InterfaceType>(type.Name, out var owner))
                return owner;

            _builder.Interface(type.Name, out owner, type.Description, null);
            if (type.Fields != null && type.Fields.Any())
                _delayedActions.Enqueue(() =>
                {
                    _builder.Connections(connect =>
                    {
                        foreach (var field in type.Fields)
                        {
                            (string Name, IType Type, object DefaultValue, string Description)[] args = field.Args
                                .Select(arg => (
                                    arg.Name,
                                    InputType(arg.Type),
                                    (object) arg.DefaultValue,
                                    arg.Description))
                                .ToArray();

                            connect.Field(
                                owner,
                                field.Name,
                                OutputType(field.Type),
                                field.Description,
                                args: args);
                        }
                    });
                });

            return owner;
        }

        private ObjectType Object(__Type type,
            bool isQueryType = false,
            bool isMutationType = false,
            bool isSubscriptionType = false)
        {
            if (_builder.TryGetType<ObjectType>(type.Name, out var owner))
                return owner;

            var interfaces = type.Interfaces?.Select(Interface)
                .ToList();

            if (isQueryType)
                _builder.Query(
                    out owner,
                    type.Description,
                    interfaces);
            else if (isMutationType)
                _builder.Mutation(
                    out owner,
                    type.Description,
                    interfaces);
            else if (isSubscriptionType)
                _builder.Subscription(
                    out owner,
                    type.Description,
                    interfaces);
            else
                _builder.Object(
                    type.Name,
                    out owner,
                    type.Description,
                    interfaces);

            if (type.Fields != null && type.Fields.Any())
                _delayedActions.Enqueue(() =>
                {
                    _builder.Connections(connect =>
                    {
                        foreach (var field in type.Fields)
                        {
                            (string Name, IType Type, object DefaultValue, string Description)[] args = field.Args
                                .Select(arg => (
                                    arg.Name,
                                    InputType(arg.Type),
                                    (object) arg.DefaultValue,
                                    arg.Description))
                                .ToArray();

                            connect.Field(
                                owner,
                                field.Name,
                                OutputType(field.Type),
                                field.Description,
                                args: args);
                        }
                    });
                });

            return owner;
        }

        private UnionType Union(__Type type)
        {
            if (_builder.TryGetType<UnionType>(type.Name, out var unionType))
                return unionType;

            var possibleTypes = type.PossibleTypes?
                .Select(possibleType => (ObjectType) OutputType(possibleType))
                .ToArray();

            _builder.Union(
                type.Name,
                out unionType,
                type.Description,
                possibleTypes: possibleTypes);

            return unionType;
        }
    }
}