﻿using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class TypeSystemDocumentExtensions
    {
        public static TypeSystemDocument WithSchemaDefinitions(
            this TypeSystemDocument document, IReadOnlyCollection<SchemaDefinition>? definitions)
        {
            return new TypeSystemDocument(
                definitions,
                document.TypeDefinitions,
                document.DirectiveDefinitions,
                document.SchemaExtensions,
                document.TypeExtensions);
        }

        public static TypeSystemDocument WithTypeDefinitions(
            this TypeSystemDocument document, IReadOnlyCollection<TypeDefinition>? definitions)
        {
            return new TypeSystemDocument(
                document.SchemaDefinitions,
                definitions,
                document.DirectiveDefinitions,
                document.SchemaExtensions,
                document.TypeExtensions);
        }

        public static TypeSystemDocument WithDirectiveDefinitions(
            this TypeSystemDocument document, IReadOnlyCollection<DirectiveDefinition>? definitions)
        {
            return new TypeSystemDocument(
                document.SchemaDefinitions,
                document.TypeDefinitions,
                definitions,
                document.SchemaExtensions,
                document.TypeExtensions);
        }

        public static TypeSystemDocument WithSchemaExtensions(
            this TypeSystemDocument document, IReadOnlyCollection<SchemaExtension>? definitions)
        {
            return new TypeSystemDocument(
                document.SchemaDefinitions,
                document.TypeDefinitions,
                document.DirectiveDefinitions,
                definitions,
                document.TypeExtensions);
        }

        public static TypeSystemDocument WithTypeExtensions(
            this TypeSystemDocument document, IReadOnlyCollection<TypeExtension>? definitions)
        {
            return new TypeSystemDocument(
                document.SchemaDefinitions,
                document.TypeDefinitions,
                document.DirectiveDefinitions,
                document.SchemaExtensions,
                definitions);
        }
    }
}