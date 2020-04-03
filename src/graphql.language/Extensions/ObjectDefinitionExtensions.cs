using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class ObjectDefinitionExtensions
    {
        public static ObjectDefinition WithDescription(this ObjectDefinition definition, 
            in StringValue? description)
        {
            return new ObjectDefinition(
                description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithName(this ObjectDefinition definition, 
            in Name name)
        {
            return new ObjectDefinition(
                definition.Description,
                name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithInterfaces(this ObjectDefinition definition,
            IReadOnlyCollection<NamedType>? interfaces)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithDirectives(this ObjectDefinition definition,
            IReadOnlyCollection<Directive>? directives)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                directives,
                definition.Fields,
                definition.Location);
        }

        public static ObjectDefinition WithFields(this ObjectDefinition definition,
            IReadOnlyCollection<FieldDefinition>? fields)
        {
            return new ObjectDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                fields,
                definition.Location);
        }
    }
}