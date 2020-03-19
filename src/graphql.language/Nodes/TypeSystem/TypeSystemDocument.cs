using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument
    {
        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<ITypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<SchemaExtension>? schemaExtensions,
            IReadOnlyCollection<ITypeDefinition>? typeExtensions)
        {
            SchemaDefinitions = schemaDefinitions;
            TypeDefinitions = typeDefinitions;
            DirectiveDefinitions = directiveDefinitions;
            SchemaExtensions = schemaExtensions;
            TypeExtensions = typeExtensions;
        }

        public IReadOnlyCollection<SchemaDefinition>? SchemaDefinitions { get; }
        public IReadOnlyCollection<ITypeDefinition>? TypeDefinitions { get; }
        public IReadOnlyCollection<DirectiveDefinition>? DirectiveDefinitions { get; }
        public IReadOnlyCollection<SchemaExtension>? SchemaExtensions { get; }
        public IReadOnlyCollection<ITypeDefinition>? TypeExtensions { get; }
    }
}