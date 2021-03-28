using System;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental
{
    public static class TypeIs
    {
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