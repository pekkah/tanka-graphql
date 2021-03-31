using System;
using System.Linq;
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
    }
}