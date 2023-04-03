using System;
using System.Diagnostics.CodeAnalysis;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class TypeDefinitionExtensions
{
    public static bool HasDirective(
        this TypeDefinition definition,
        Name directiveName)
    {
        if (definition.Directives is null) return false;

        return definition.Directives.TryGet(directiveName, out _);
    }

    public static bool TryGetDirective(
        this TypeDefinition definition,
        Name directiveName,
        [NotNullWhen(true)] out Directive? directive)
    {
        if (definition.Directives is null)
        {
            directive = null;
            return false;
        }

        return definition.Directives.TryGet(directiveName, out directive);
    }

    public static TypeDefinition Extend(
        this TypeDefinition typeDefinition,
        params TypeExtension[] typeExtensions)
    {
        return typeDefinition switch
        {
            EnumDefinition enumDefinition => Extend(enumDefinition, typeExtensions),
            InputObjectDefinition inputObjectDefinition => Extend(inputObjectDefinition, typeExtensions),
            InterfaceDefinition interfaceDefinition => Extend(interfaceDefinition, typeExtensions),
            ObjectDefinition objectDefinition => Extend(objectDefinition, typeExtensions),
            ScalarDefinition scalarDefinition => Extend(scalarDefinition, typeExtensions),
            UnionDefinition unionDefinition => Extend(unionDefinition, typeExtensions),
            _ => throw new ArgumentOutOfRangeException(nameof(typeDefinition))
        };
    }

    public static UnionDefinition Extend(
        this UnionDefinition unionDefinition,
        params TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(unionDefinition, extension);

            var extensionDefinition = (UnionDefinition)extension.Definition;
            unionDefinition = unionDefinition
                .WithDirectives(
                    unionDefinition.Directives
                        .Concat(extensionDefinition.Directives)
                )
                .WithMembers(
                    unionDefinition.Members
                        .Join(extensionDefinition.Members)
                );
        }

        return unionDefinition;
    }

    public static ScalarDefinition Extend(
        this ScalarDefinition scalarDefinition,
        TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(scalarDefinition, extension);

            var extensionDefinition = (ScalarDefinition)extension.Definition;
            scalarDefinition = scalarDefinition
                .WithDirectives(
                    scalarDefinition.Directives
                        .Concat(extensionDefinition.Directives)
                );
        }

        return scalarDefinition;
    }

    public static ObjectDefinition Extend(
        this ObjectDefinition objectDefinition,
        TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(objectDefinition, extension);

            var extensionDefinition = (ObjectDefinition)extension.Definition;

            var extendedDirectives = objectDefinition.Directives
                .Concat(extensionDefinition.Directives);

            var extendedFields = objectDefinition.Fields
                .Join(extensionDefinition.Fields);

            var extendedInterfaces = objectDefinition.Interfaces
                .Join(extensionDefinition.Interfaces);

            objectDefinition = new ObjectDefinition(
                objectDefinition.Description,
                objectDefinition.Name,
                ImplementsInterfaces.From(extendedInterfaces),
                extendedDirectives,
                extendedFields,
                objectDefinition.Location);
        }

        return objectDefinition;
    }

    public static InterfaceDefinition Extend(
        this InterfaceDefinition interfaceDefinition,
        TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(interfaceDefinition, extension);

            var extensionDefinition = (InterfaceDefinition)extension.Definition;

            var extendedDirectives = interfaceDefinition.Directives
                .Concat(extensionDefinition.Directives);

            var extendedFields = interfaceDefinition.Fields
                .Join(extensionDefinition.Fields);

            var extendedInterfaces = interfaceDefinition.Interfaces
                .Join(extensionDefinition.Interfaces);

            interfaceDefinition = new InterfaceDefinition(
                interfaceDefinition.Description,
                interfaceDefinition.Name,
                ImplementsInterfaces.From(extendedInterfaces),
                extendedDirectives,
                extendedFields,
                interfaceDefinition.Location);
        }

        return interfaceDefinition;
    }

    public static InputObjectDefinition Extend(
        this InputObjectDefinition inputObjectDefinition,
        TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(inputObjectDefinition, extension);

            var extensionDefinition = (InputObjectDefinition)extension.Definition;

            var extendedDirectives = inputObjectDefinition.Directives
                .Concat(extensionDefinition.Directives);

            var extendedFields = inputObjectDefinition.Fields
                .Join(extensionDefinition.Fields);

            inputObjectDefinition = new InputObjectDefinition(
                inputObjectDefinition.Description,
                inputObjectDefinition.Name,
                extendedDirectives,
                extendedFields,
                inputObjectDefinition.Location);
        }

        return inputObjectDefinition;
    }

    public static EnumDefinition Extend(
        this EnumDefinition enumDefinition,
        params TypeExtension[] typeExtensions)
    {
        foreach (var extension in typeExtensions)
        {
            EnsureExtendedType(enumDefinition, extension);

            var extensionDefinition = (EnumDefinition)extension.Definition;

            var extendedDirectives = enumDefinition.Directives
                .Concat(extensionDefinition.Directives);

            var extendedValues = enumDefinition.Values
                .Join(extensionDefinition.Values);

            enumDefinition = new EnumDefinition(
                enumDefinition.Description,
                enumDefinition.Name,
                extendedDirectives,
                extendedValues,
                enumDefinition.Location);
        }

        return enumDefinition;
    }

    private static void EnsureExtendedType(TypeDefinition typeDefinition, TypeExtension typeExtension)
    {
        if (typeDefinition.Kind != typeExtension.ExtendedKind)
            throw new InvalidOperationException(
                $"Invalid type extension '{typeExtension.ExtendedKind}' for type '{typeDefinition.Kind}'.");
    }
}