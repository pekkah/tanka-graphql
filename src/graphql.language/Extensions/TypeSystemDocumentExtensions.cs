using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class TypeSystemDocumentExtensions
    {
        public static TypeSystemDocument Merge(this TypeSystemDocument left, TypeSystemDocument right)
        {
            var schemaDefinitions = left.SchemaDefinitions ?? Array.Empty<SchemaDefinition>();
            var typeDefinitions = left.TypeDefinitions ?? Array.Empty<TypeDefinition>();
            var directiveDefinitions = left.DirectiveDefinitions ?? Array.Empty<DirectiveDefinition>();
            var schemaExtensions = left.SchemaExtensions ?? Array.Empty<SchemaExtension>();
            var typeExtensions = left.TypeExtensions ?? Array.Empty<TypeExtension>();
            var imports = left.Imports ?? Array.Empty<Import>();

            return new TypeSystemDocument(
                right.SchemaDefinitions != null
                    ? schemaDefinitions.Concat(right.SchemaDefinitions).ToList()
                    : schemaDefinitions,
                right.TypeDefinitions != null
                    ? typeDefinitions.Concat(right.TypeDefinitions).ToList()
                    : typeDefinitions,
                right.DirectiveDefinitions != null
                    ? directiveDefinitions.Concat(right.DirectiveDefinitions).ToList()
                    : directiveDefinitions,
                right.SchemaExtensions != null
                    ? schemaExtensions.Concat(right.SchemaExtensions).ToList()
                    : schemaExtensions,
                right.TypeExtensions != null
                    ? typeExtensions.Concat(right.TypeExtensions).ToList()
                    : typeExtensions,
                right.Imports != null
                    ? imports.Concat(right.Imports).ToList()
                    : imports
            );
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