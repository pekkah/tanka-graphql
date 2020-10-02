using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class SchemaExtensionExtensions
    {
        public static SchemaExtension WithDescription(this SchemaExtension definition, 
            in StringValue? description)
        {
            return new SchemaExtension(
                description,
                definition.Directives,
                definition.Operations,
                definition.Location);
        }
        public static SchemaExtension WithDirectives(this SchemaExtension definition,
            Directives? directives)
        {
            return new SchemaExtension(
                definition.Description,
                directives,
                definition.Operations,
                definition.Location);
        }

        public static SchemaExtension WithOperations(this SchemaExtension definition,
            IReadOnlyList<RootOperationTypeDefinition>? operations)
        {
            return new SchemaExtension(
                definition.Description,
                definition.Directives,
                operations,
                definition.Location);
        }
    }
}