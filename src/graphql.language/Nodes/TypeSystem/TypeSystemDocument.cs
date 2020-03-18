using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument
    {
        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<ITypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<ISchemaExtension>? schemaExtensions,
            IReadOnlyCollection<ITypeExtension>? typeExtensions)
        {
      
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