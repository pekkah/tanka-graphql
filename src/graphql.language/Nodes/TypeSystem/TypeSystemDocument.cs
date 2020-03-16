using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class TypeSystemDocument
    {
        public TypeSystemDocument(
            IReadOnlyCollection<SchemaDefinition>? schemaDefinitions,
            IReadOnlyCollection<TypeDefinition>? typeDefinitions,
            IReadOnlyCollection<DirectiveDefinition>? directiveDefinitions,
            IReadOnlyCollection<SchemaExtension>? schemaExtensions,
            IReadOnlyCollection<TypeExtension>? typeExtensions)
        {
      
        }
    }

    public sealed class SchemaDefinition
    {

    }

    public sealed class TypeDefinition
    {

    }

    public sealed class DirectiveDefinition
    {

    }

    public sealed class SchemaExtension
    {

    }

    public sealed class TypeExtension
    {

    }
}