using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Experimental.Definitions;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental.TypeSystem
{
    public static class Ast
    {
        public static TypeDefinition? TypeFromAst(ExecutableSchema schema, TypeBase? type)
        {
            if (type == null)
                return null;

            if (type.Kind == NodeKind.NonNullType)
            {
                var innerType = TypeFromAst(schema, ((NonNullType) type).OfType);
                return innerType;
            }

            if (type.Kind == NodeKind.ListType)
            {
                var innerType = TypeFromAst(schema, ((ListType) type).OfType);
                return innerType;
            }

            if (type.Kind == NodeKind.NamedType)
            {
                var namedType = (NamedType) type;
                var typeDefinition = schema.GetNamedType<TypeDefinition>(namedType.Name);

                return typeDefinition;
            }

            throw new Exception($"Unexpected type kind: {type.Kind}");
        }

        public static OperationDefinition GetOperation(ExecutableDocument document, string? operationName)
        {
            var operations = document.OperationDefinitions;

            if (operations == null || operations.Count == 0)
                throw new Exception("Document does not contain operations");

            if (string.IsNullOrEmpty(operationName))
            {
                if (operations.Count == 1) return operations.Single();

                throw new Exception(
                    "Multiple operations found. Please provide OperationName");
            }

            var operation = operations.SingleOrDefault(op => op.Name.Value == operationName);

            if (operation == null)
                throw new Exception(
                    $"Could not find operation with name {operationName}");

            return operation;
        }

        public static bool IsInputType(ExecutableSchema schema, TypeBase type)
        {
            return type switch
            {
                ListType listType => IsInputType(schema, listType.OfType),
                NamedType namedType => IsInputType(schema, namedType),
                NonNullType nonNullType => IsInputType(schema, nonNullType.OfType),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static bool IsInputType(ExecutableSchema schema, NamedType namedType)
        {
            var typeDefinition = schema.GetNamedType<TypeDefinition>(namedType.Name);

            return typeDefinition != null && typeDefinition.Kind switch
            {
                NodeKind.ScalarDefinition => true,
                NodeKind.EnumDefinition => true,
                NodeKind.InputObjectDefinition => true,
                _ => false
            };
        }

        public static bool IsOutputType(ExecutableSchema schema, TypeBase type)
        {
            return type switch
            {
                ListType listType => IsOutputType(schema, listType.OfType),
                NamedType namedType => IsOutputType(schema, namedType),
                NonNullType nonNullType => IsOutputType(schema, nonNullType.OfType),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public static bool IsOutputType(ExecutableSchema schema, NamedType namedType)
        {
            var typeDefinition = schema.GetNamedType<TypeDefinition>(namedType.Name);

            return typeDefinition != null && typeDefinition.Kind switch
            {
                NodeKind.ScalarDefinition => true,
                NodeKind.ObjectDefinition => true,
                NodeKind.InterfaceDefinition => true,
                NodeKind.UnionDefinition => true,
                NodeKind.EnumDefinition => true,
                _ => false
            };
        }

        public static bool GetIfArgumentValue(
            ExecutableSchema schema,
            Directive directive,
            IReadOnlyDictionary<string, object?> coercedVariableValues,
            Argument argument,
            CoerceValue coerceValue)
        {
            if (directive == null) throw new ArgumentNullException(nameof(directive));
            if (coercedVariableValues == null) throw new ArgumentNullException(nameof(coercedVariableValues));

            switch (argument.Value.Kind)
            {
                case NodeKind.BooleanValue:
                    return (bool) (coerceValue(schema, argument.Value, "Boolean!") ?? false);
                case NodeKind.Variable:
                    var variable = (Variable) argument.Value;
                    var variableValue = coercedVariableValues[variable.Name];

                    if (variableValue == null)
                        throw new Exception(
                            $"If argument of {directive} is null. Variable value should not be null");

                    return (bool) variableValue;
                default:
                    return false;
            }
        }

        public static bool DoesFragmentTypeApply(
            ObjectDefinition objectType,
            TypeDefinition fragmentType)
        {
            if (objectType.Name == fragmentType.Name)
                return true;

            if (fragmentType is InterfaceDefinition interfaceType)
                return objectType
                    .Interfaces
                    ?.Any(implementedInterface => implementedInterface.Name == interfaceType.Name) == true;

            if (fragmentType is UnionDefinition unionType)
                return unionType
                    .Members
                    ?.Any(member => member.Name == objectType.Name) == true;

            return false;
        }
    }
}