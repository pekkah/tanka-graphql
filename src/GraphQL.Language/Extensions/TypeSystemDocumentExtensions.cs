﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class TypeSystemDocumentExtensions
{
    public static IEnumerable<NamedType> GetNamedTypes(this TypeSystemDocument document)
    {
        if (document.DirectiveDefinitions is not null)
            foreach (var definition in document.DirectiveDefinitions)
            {
                yield return new NamedType(definition.Name, definition.Location);
            }

        if (document.TypeDefinitions is not null)
            foreach (var definition in document.TypeDefinitions)
            {
                yield return new NamedType(definition.Name, definition.Location);
            }
    }


    public static TypeSystemDocument WithTypeSystem(this TypeSystemDocument left, TypeSystemDocument right)
    {
        var schemaDefinitions = left.SchemaDefinitions ?? Array.Empty<SchemaDefinition>();
        var typeDefinitions = left.TypeDefinitions ?? Array.Empty<TypeDefinition>();
        var directiveDefinitions = left.DirectiveDefinitions ?? Array.Empty<DirectiveDefinition>();
        var schemaExtensions = left.SchemaExtensions ?? Array.Empty<SchemaExtension>();
        var typeExtensions = left.TypeExtensions ?? Array.Empty<TypeExtension>();
        var imports = left.Imports ?? Array.Empty<Import>();

        return new TypeSystemDocument(
            right.SchemaDefinitions != null
                ? (schemaDefinitions.Concat(right.SchemaDefinitions) ?? Array.Empty<SchemaDefinition>()).ToList()
                : schemaDefinitions,
            right.TypeDefinitions != null
                ? (typeDefinitions.Concat(right.TypeDefinitions) ?? Array.Empty<TypeDefinition>()).ToList()
                : typeDefinitions,
            right.DirectiveDefinitions != null
                ? (directiveDefinitions.Concat(right.DirectiveDefinitions) ?? Array.Empty<DirectiveDefinition>()).ToList()
                : directiveDefinitions,
            right.SchemaExtensions != null
                ? (schemaExtensions.Concat(right.SchemaExtensions) ?? Array.Empty<SchemaExtension>()).ToList()
                : schemaExtensions,
            right.TypeExtensions != null
                ? (typeExtensions.Concat(right.TypeExtensions) ?? Array.Empty<TypeExtension>()).ToList()
                : typeExtensions,
            right.Imports != null
                ? (imports.Concat(right.Imports) ?? Array.Empty<Import>()).ToList()
                : imports
        );
    }

    public static TypeSystemDocument WithImports(
        this TypeSystemDocument document,
        IReadOnlyList<Import>? definitions)
    {
        return new TypeSystemDocument(
            document.SchemaDefinitions,
            document.TypeDefinitions,
            document.DirectiveDefinitions,
            document.SchemaExtensions,
            document.TypeExtensions,
            definitions);
    }

    public static TypeSystemDocument WithDirectiveDefinitions(
        this TypeSystemDocument document, IReadOnlyList<DirectiveDefinition>? definitions)
    {
        return new TypeSystemDocument(
            document.SchemaDefinitions,
            document.TypeDefinitions,
            definitions,
            document.SchemaExtensions,
            document.TypeExtensions);
    }

    public static TypeSystemDocument WithSchemaDefinitions(
        this TypeSystemDocument document, IReadOnlyList<SchemaDefinition>? definitions)
    {
        return new TypeSystemDocument(
            definitions,
            document.TypeDefinitions,
            document.DirectiveDefinitions,
            document.SchemaExtensions,
            document.TypeExtensions);
    }

    public static TypeSystemDocument WithSchemaExtensions(
        this TypeSystemDocument document, IReadOnlyList<SchemaExtension>? definitions)
    {
        return new TypeSystemDocument(
            document.SchemaDefinitions,
            document.TypeDefinitions,
            document.DirectiveDefinitions,
            definitions,
            document.TypeExtensions);
    }

    public static TypeSystemDocument WithTypeDefinitions(
        this TypeSystemDocument document, IReadOnlyList<TypeDefinition>? definitions)
    {
        return new TypeSystemDocument(
            document.SchemaDefinitions,
            definitions,
            document.DirectiveDefinitions,
            document.SchemaExtensions,
            document.TypeExtensions);
    }

    public static TypeSystemDocument WithTypeExtensions(
        this TypeSystemDocument document, IReadOnlyList<TypeExtension>? definitions)
    {
        return new TypeSystemDocument(
            document.SchemaDefinitions,
            document.TypeDefinitions,
            document.DirectiveDefinitions,
            document.SchemaExtensions,
            definitions);
    }
}