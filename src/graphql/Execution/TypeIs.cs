﻿using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution;

public static class TypeIs
{
    public static bool IsInputType(ISchema schema, TypeBase type)
    {
        return type switch
        {
            NonNullType NonNullType => IsInputType(schema, NonNullType.OfType),
            ListType list => IsInputType(schema, list.OfType),
            NamedType namedType => IsInputType(schema.GetRequiredNamedType<TypeDefinition>(namedType.Name)),
            _ => false
        };
    }

    public static bool IsInputType(TypeDefinition type)
    {
        return type switch
        {
            ScalarDefinition => true,
            EnumDefinition => true,
            InputObjectDefinition => true,
            _ => false
        };
    }

    public static bool IsOutputType(ISchema schema, TypeBase type)
    {
        return type switch
        {
            NonNullType NonNullType => IsOutputType(schema, NonNullType.OfType),
            ListType list => IsOutputType(schema, list.OfType),
            NamedType namedType => IsOutputType(schema.GetRequiredNamedType<TypeDefinition>(namedType.Name)),
            _ => false
        };
    }

    public static bool IsOutputType(TypeDefinition type)
    {
        return type switch
        {
            ScalarDefinition => true,
            ObjectDefinition => true,
            InterfaceDefinition => true,
            UnionDefinition => true,
            EnumDefinition => true,
            _ => false
        };
    }
}