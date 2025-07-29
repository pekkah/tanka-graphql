using System;
using System.Collections.Generic;
using System.Linq;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Introspection;

public static class IntrospectionSchemaBuilderExtensions
{
    public static SchemaBuilder AddIntrospectedSchema(this SchemaBuilder builder, __Schema schema)
    {
        foreach (var schemaType in schema.Types) builder.Add(schemaType);

        return builder;
    }

    public static SchemaBuilder Add(this SchemaBuilder builder, __Type type)
    {
        return type.Kind switch
        {
            __TypeKind.SCALAR => builder.AddScalarDefinition(type),
            __TypeKind.OBJECT => builder.AddObjectDefinition(type),
            __TypeKind.INTERFACE => builder.AddInterfaceDefinition(type),
            __TypeKind.UNION => builder.AddUnionDefinition(type),
            __TypeKind.ENUM => builder.AddEnumDefinition(type),
            __TypeKind.INPUT_OBJECT => builder.AddInputObjectDefinition(type),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type.Kind, "Cannot add as schema type")
        };
    }

    public static SchemaBuilder AddScalarDefinition(this SchemaBuilder builder, __Type type)
    {
        builder.AddTypeExtension(
            new ScalarDefinition(
                type.Description,
                type.Name)
        );
        return builder;
    }

    public static SchemaBuilder AddObjectDefinition(this SchemaBuilder builder, __Type type)
    {
        return builder.AddTypeExtension(
            new ObjectDefinition(
                type.Description,
                type.Name,
                MapInterfaces(type.Interfaces),
                null,
                MapFields(type.Fields))
        );
    }

    public static SchemaBuilder AddInterfaceDefinition(this SchemaBuilder builder, __Type type)
    {
        return builder.AddTypeExtension(
            new InterfaceDefinition(
                type.Description,
                type.Name,
                MapInterfaces(type.Interfaces),
                null,
                MapFields(type.Fields))
        );
    }

    public static SchemaBuilder AddUnionDefinition(this SchemaBuilder builder, __Type type)
    {
        return builder.AddTypeExtension(
            new UnionDefinition(
                type.Description,
                type.Name,
                null,
                MapMembers(type))
        );
    }

    public static SchemaBuilder AddEnumDefinition(this SchemaBuilder builder, __Type type)
    {
        return builder.AddTypeExtension(
            new EnumDefinition(
                type.Description,
                type.Name,
                null,
                MapValues(type))
        );
    }

    public static SchemaBuilder AddInputObjectDefinition(this SchemaBuilder builder, __Type type)
    {
        return builder.AddTypeExtension(
            new InputObjectDefinition(
                type.Description,
                type.Name,
                null,
                MapInputFields(type))
        );
    }

    private static SchemaBuilder AddTypeExtension(this SchemaBuilder builder, TypeDefinition extendedType)
    {
        return builder.Add(new[]
        {
            new TypeExtension(extendedType)
        });
    }

    private static SchemaBuilder AddTypeExtensions(this SchemaBuilder builder,
        IEnumerable<TypeDefinition> extendedTypes)
    {
        foreach (var typeDefinition in extendedTypes) builder.AddTypeExtension(typeDefinition);

        return builder;
    }

    private static UnionMemberTypes MapMembers(__Type type)
    {
        return new UnionMemberTypes(
            type.PossibleTypes
                .Select(m => new NamedType(m.Name))
                .ToList());
    }

    private static EnumValuesDefinition MapValues(__Type type)
    {
        return new EnumValuesDefinition(
            type.EnumValues.Select(ev => new EnumValueDefinition(
                ev.Description,
                new EnumValue(ev.Name),
                null)).ToList());
    }

    private static InputFieldsDefinition MapInputFields(__Type type)
    {
        return new InputFieldsDefinition(
            type.InputFields.Select(f => new InputValueDefinition(
                f.Description,
                f.Name,
                MapTypeBase(f.Type),
                MapDefaultValue(f.DefaultValue))).ToList());
    }

    private static DefaultValue MapDefaultValue(string defaultValue)
    {
        return new DefaultValue(Parser.Create(defaultValue).ParseValue(true));
    }

    private static ImplementsInterfaces MapInterfaces(List<__Type> typeInterfaces)
    {
        return new ImplementsInterfaces(
            typeInterfaces.Select(i => new NamedType(i.Name)).ToList());
    }

    private static FieldsDefinition MapFields(List<__Field> typeFields)
    {
        var fields = typeFields
            .Select(f => new FieldDefinition(
                f.Description,
                f.Name,
                MapArguments(f.Args),
                MapTypeBase(f.Type)))
            .ToList();

        return new FieldsDefinition(fields);
    }

    private static ArgumentsDefinition MapArguments(List<__InputValue> typeArgs)
    {
        return new ArgumentsDefinition(
            typeArgs.Select(a => new InputValueDefinition(
                a.Description,
                a.Name,
                MapTypeBase(a.Type))).ToList());
    }

    private static TypeBase MapTypeBase(__Type argType)
    {
        return argType.Kind switch
        {
            __TypeKind.NON_NULL => new NonNullType(MapTypeBase(argType.OfType)),
            __TypeKind.LIST => new ListType(MapTypeBase(argType.OfType)),
            _ => new NamedType(argType.Name)
        };
    }
}