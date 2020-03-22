using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class ObjectDefinitionExtensions
    {
        public static ObjectDefinition WithName(this ObjectDefinition objectDefinition, in Name name)
        {
            return new ObjectDefinition(
                objectDefinition.Description,
                name,
                objectDefinition.Interfaces,
                objectDefinition.Directives,
                objectDefinition.Fields,
                objectDefinition.Location);
        }

        public static ObjectDefinition WithFields(this ObjectDefinition objectDefinition, IReadOnlyCollection<FieldDefinition>? fields)
        {
            return new ObjectDefinition(
                objectDefinition.Description,
                objectDefinition.Name,
                objectDefinition.Interfaces,
                objectDefinition.Directives,
                fields,
                objectDefinition.Location);
        }
    }
}