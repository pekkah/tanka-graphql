using System;
using System.Collections.Generic;
using System.Linq;
using fugu.graphql.error;
using fugu.graphql.execution;
using fugu.graphql.tools;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.sdl
{
    public static class Sdl
    {
        public static ObjectType Object(GraphQLObjectTypeDefinition definition, SdlParserContext context)
        {
            context.PushObject(definition);

            var fields = Fields(definition.Fields, context);
            var interfaces = Interfaces(definition.Interfaces, context);
            var result = new ObjectType(
                definition.Name.Value,
                fields,
                implements: interfaces);

            context.KnownTypes.Add(result);
            context.PopObject();
            return result;
        }

        public static IField Field(GraphQLFieldDefinition definition, SdlParserContext context)
        {
            var args = Args(definition.Arguments, context);
            return new Field(
                Type(definition.Type, context),
                args,
                new Meta(directives: Directives(definition.Directives, context)));
        }

        public static IEnumerable<IGraphQLType> Document(GraphQLDocument document, SdlParserContext context)
        {
            foreach (var definition in document.Definitions.OfType<GraphQLScalarTypeDefinition>())
                Scalar(definition, context);

            foreach (var directiveDefinition in document.Definitions.OfType<GraphQLDirectiveDefinition>())
                DirectiveType(directiveDefinition, context);

            foreach (var definition in document.Definitions.OfType<GraphQLInputObjectTypeDefinition>())
                InputObject(definition, context);

            foreach (var definition in document.Definitions.OfType<GraphQLEnumTypeDefinition>())
                Enum(definition, context);

            foreach (var definition in document.Definitions.OfType<GraphQLInterfaceTypeDefinition>())
                Interface(definition, context);

            foreach (var definition in document.Definitions.OfType<GraphQLObjectTypeDefinition>())
                Object(definition, context);


            foreach (var definition in document.Definitions.OfType<GraphQLTypeExtensionDefinition>())
                Extend(definition, context);

            return context.KnownTypes;
        }

        public static InputObjectType InputObject(GraphQLInputObjectTypeDefinition definition, SdlParserContext context)
        {
            var fields = InputValues(definition.Fields, context);

            var result = new InputObjectType(
                definition.Name.Value,
                fields);

            context.KnownTypes.Add(result);
            return result;
        }

        public static InterfaceType Interface(GraphQLInterfaceTypeDefinition definition, SdlParserContext context)
        {
            context.PushInterface(definition);

            var fields = Fields(definition.Fields, context);
            var directives = Directives(definition.Directives, context);
            var result = new InterfaceType(
                definition.Name.Value,
                fields,
                new Meta(directives: directives));

            context.KnownTypes.Add(result);
            context.PopInterface();
            return result;
        }

        public static EnumType Enum(GraphQLEnumTypeDefinition definition, SdlParserContext context)
        {
            var values = new EnumValues();

            foreach (var valueDefinition in definition.Values)
                values[valueDefinition.Name.Value] = new Meta(
                    directives: Directives(valueDefinition.Directives, context));

            var directives = Directives(definition.Directives, context);
            var result = new EnumType(
                definition.Name.Value,
                values,
                new Meta(directives: directives));

            context.KnownTypes.Add(result);
            return result;
        }

        public static ISchema Schema(GraphQLDocument document,
            IEnumerable<ScalarType> scalars = null,
            IEnumerable<DirectiveType> directives = null)
        {
            var knownTypes = new List<IGraphQLType>();

            if (scalars != null)
                knownTypes.AddRange(scalars);

            if (directives != null)
                knownTypes.AddRange(directives);

            var context = new SdlParserContext(document, knownTypes);

            var schemaDefinition = document.Definitions.OfType<GraphQLSchemaDefinition>().SingleOrDefault();

            if (schemaDefinition == null)
                throw new GraphQLError(
                    $"Could not find single schema definition from document", document);

            Document(document, context);

            var queryTypeName = schemaDefinition.OperationTypes.Single(o => o.Operation == OperationType.Query).Type
                .Name.Value;

            var mutationTypeName = schemaDefinition.OperationTypes
                .SingleOrDefault(o => o.Operation == OperationType.Mutation)?.Type.Name.Value;

            var subscriptionTypeName = schemaDefinition.OperationTypes
                .SingleOrDefault(o => o.Operation == OperationType.Subscription)?.Type.Name.Value;

            var queryType = (ObjectType) context.GetKnownType(queryTypeName);
            ObjectType mutationType = null;
            ObjectType subscriptionType = null;

            if (!string.IsNullOrEmpty(mutationTypeName))
                mutationType = (ObjectType) context.GetKnownType(mutationTypeName);

            if (!string.IsNullOrEmpty(subscriptionTypeName))
                subscriptionType = (ObjectType) context.GetKnownType(subscriptionTypeName);

            return new Schema(queryType, mutationType, subscriptionType);
        }

        public static ScalarType Scalar(GraphQLScalarTypeDefinition definition, SdlParserContext context)
        {
            var scalar = context.GetKnownType(definition.Name.Value) as ScalarType;

            if (scalar == null)
                throw new GraphQLError(
                    $"Scalar type '{definition.Name.Value}' not known. Add it to the context before parsing.",
                    definition);

            return scalar;
        }

        private static DirectiveType DirectiveType(GraphQLDirectiveDefinition definition, SdlParserContext context)
        {
            var directiveType = context.GetKnownType(definition.Name.Value) as DirectiveType;

            if (directiveType == null)
                throw new GraphQLError(
                    $"DirectiveType type '{definition.Name.Value}' not known. Add it to the context before parsing.",
                    definition);

            return directiveType;
        }

        private static IEnumerable<DirectiveInstance> Directives(IEnumerable<GraphQLDirective> directiveDefinitions,
            SdlParserContext context)
        {
            foreach (var directiveDefinition in directiveDefinitions)
            foreach (var directiveInstance in DirectiveInstance(context, directiveDefinition))
                yield return directiveInstance;
        }

        private static IEnumerable<DirectiveInstance> DirectiveInstance(SdlParserContext context,
            GraphQLDirective directiveDefinition)
        {
            var name = directiveDefinition.Name.Value;
            var directiveType = context.GetKnownType(name) as DirectiveType;

            if (directiveType == null)
                throw new GraphQLError(
                    $"Could not find DirectiveType with name '{name}'");

            var arguments = new Args();
            foreach (var argument in directiveType.Arguments)
            {
                var type = argument.Value.Type;
                var defaultValue = argument.Value.DefaultValue;
                var definition = directiveDefinition.Arguments.SingleOrDefault(a => a.Name.Value == argument.Key);

                var hasValue = definition != null;
                var value = definition?.Value;

                if (!hasValue && defaultValue != null)
                {
                    //todo: this needs it's own type
                    arguments.Add(argument.Key, new Argument
                    {
                        DefaultValue = defaultValue,
                        Type = type
                    });
                    continue;
                }

                if (type is NonNull
                    && (!hasValue || value == null))
                    throw new NullValueForNonNullTypeException(
                        $"Argument {argument.Key} type is non-nullable but value is null or not set", directiveType);

                if (hasValue)
                {
                    if (value == null)
                        arguments.Add(argument.Key, new Argument
                        {
                            DefaultValue = null,
                            Type = type
                        });
                    else
                        arguments.Add(argument.Key, new Argument
                        {
                            DefaultValue = Values.CoerceValue(value, type),
                            Type = type
                        });
                }
            }

            yield return new DirectiveInstance(directiveType, arguments);
        }

        private static IGraphQLType Extend(GraphQLTypeExtensionDefinition definition, SdlParserContext context)
        {
            var originalType = context.GetKnownType(definition.Definition.Name.Value);

            if (originalType == null)
                return null;

            var extensionType = Object(definition.Definition, context);
            context.KnownTypes.Remove(extensionType);

            if (originalType is ObjectType originalObjectType)
            {
                var extendedType =
                    MergeTool.Merge(
                        originalObjectType, 
                        extensionType,
                        (l, r) => r.Field); //todo: this needs fixing

                context.KnownTypes.Remove(originalType);
                context.KnownTypes.Add(extendedType);

                return extendedType;
            }

            return null;
        }

        private static IEnumerable<InterfaceType> Interfaces(IEnumerable<GraphQLNamedType> definitions, SdlParserContext context)
        {
            var interfaces = new List<InterfaceType>();

            foreach (var namedType in definitions)
            {
                var type = Type(namedType, context) as InterfaceType;

                if (type == null)
                    continue;

                interfaces.Add(type);
            }

            return interfaces;
        }

        private static Fields Fields(IEnumerable<GraphQLFieldDefinition> definitions, SdlParserContext context)
        {
            var fields = new Fields();
            foreach (var definition in definitions)
                fields[definition.Name.Value] = Field(definition, context);

            return fields;
        }

        private static Args Args(IEnumerable<GraphQLInputValueDefinition> definitions, SdlParserContext context)
        {
            var args = new Args();

            foreach (var definition in definitions)
            {
                var type = Type(definition.Type, context);
                object defaultValue;

                try
                {
                    defaultValue = Values.CoerceValue(definition.DefaultValue, type);
                }
                catch (Exception)
                {
                    defaultValue = null;
                }

                args[definition.Name.Value] = new Argument
                {
                    Type = type,
                    DefaultValue = defaultValue
                };
            }

            return args;
        }

        private static IGraphQLType Type(GraphQLType typeDefinition, SdlParserContext context)
        {
            if (typeDefinition.Kind == ASTNodeKind.NonNullType)
            {
                var innerType = Type(((GraphQLNonNullType) typeDefinition).Type, context);
                return innerType != null ? new NonNull(innerType) : null;
            }

            if (typeDefinition.Kind == ASTNodeKind.ListType)
            {
                var innerType = Type(((GraphQLListType) typeDefinition).Type, context);
                return innerType != null ? new List(innerType) : null;
            }

            var namedTypeDefinition = (GraphQLNamedType) typeDefinition;
            var typeName = namedTypeDefinition.Name.Value;

            // avoid getting caught in circular graph
            if (context.IsBeingBuilt(typeName))
                return new NamedTypeReference(typeName);

            // is known type?
            var type = context.GetKnownType(typeName);

            if (type != null)
                return type;

            return new NamedTypeReference(typeName);
        }

        private static InputFields InputValues(IEnumerable<GraphQLInputValueDefinition> definitions, SdlParserContext context)
        {
            var fields = new InputFields();

            foreach (var definition in definitions)
            {
                var type = Type(definition.Type, context);
                object defaultValue = null;

                try
                {
                    defaultValue = Values.CoerceValue(definition.DefaultValue, type);
                }
                catch (NullValueForNonNullTypeException)
                {
                    defaultValue = null;
                }

                if (!Validations.IsInputType(type))
                {
                    throw new GraphQLError($"Type of input value definition is not valid input value type. " +
                                           $"Definition: '{definition.Name.Value}' Type: {definition.Type.Kind}",
                        definition);
                }

                var directives = Directives(definition.Directives, context);

                switch (type)
                {
                    case ScalarType scalarType:
                        fields[definition.Name.Value] = new InputObjectField(
                            scalarType,
                            new Meta(directives: directives),
                            defaultValue: defaultValue);
                        break;
                    case EnumType enumType:
                        fields[definition.Name.Value] = new InputObjectField(
                            enumType, 
                            new Meta(directives: directives),
                            defaultValue: defaultValue);
                        break;
                    case InputObjectType inputObjectType:
                        fields[definition.Name.Value] = new InputObjectField(
                            inputObjectType, 
                            new Meta(directives: directives),
                            defaultValue: defaultValue);
                        break;
                    case NonNull nonNull:
                        fields[definition.Name.Value] = new InputObjectField(
                            nonNull,
                            new Meta(directives: directives),
                            defaultValue: defaultValue);
                        break;
                    case List list:
                        fields[definition.Name.Value] = new InputObjectField(
                            list,
                            new Meta(directives: directives),
                            defaultValue: defaultValue);
                        break;
                }
            }

            return fields;
        }
    }
}