using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument
    {
        public IReadOnlyCollection<SchemaDefinition>? SchemaDefinitions { get; }
        public IReadOnlyCollection<ITypeDefinition>? TypeDefinitions { get; }
        public IReadOnlyCollection<DirectiveDefinition>? DirectiveDefinitions { get; }
        public IReadOnlyCollection<ISchemaExtension>? SchemaExtensions { get; }
        public IReadOnlyCollection<ITypeExtension>? TypeExtensions { get; }

        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<ITypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<ISchemaExtension>? schemaExtensions,
            IReadOnlyCollection<ITypeExtension>? typeExtensions)
        {
            SchemaDefinitions = schemaDefinitions;
            TypeDefinitions = typeDefinitions;
            DirectiveDefinitions = directiveDefinitions;
            SchemaExtensions = schemaExtensions;
            TypeExtensions = typeExtensions;
        }
    }

    public interface ISchemaExtension
    {

    }

    public interface ITypeExtension
    {

    }

    public interface ITypeDefinition
    {

    }
}