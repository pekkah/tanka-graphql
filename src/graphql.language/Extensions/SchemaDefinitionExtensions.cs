﻿using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class SchemaDefinitionExtensions
    {
        public static SchemaDefinition WithDescription(this SchemaDefinition definition, 
            in StringValue? description)
        {
            return new SchemaDefinition(
                description,
                definition.Directives,
                definition.Operations,
                definition.Location);
        }
        public static SchemaDefinition WithDirectives(this SchemaDefinition definition,
            Directives? directives)
        {
            return new SchemaDefinition(
                definition.Description,
                directives,
                definition.Operations,
                definition.Location);
        }

        public static SchemaDefinition WithOperations(this SchemaDefinition definition,
            RootOperationTypeDefinitions operations)
        {
            return new SchemaDefinition(
                definition.Description,
                definition.Directives,
                operations,
                definition.Location);
        }
    }
}