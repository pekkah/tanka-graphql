using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class InterfaceDefinitionExtensions
    {
        public static InterfaceDefinition WithDescription(this InterfaceDefinition definition, 
            in StringValue? description)
        {
            return new InterfaceDefinition(
                description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithName(this InterfaceDefinition definition, 
            in Name name)
        {
            return new InterfaceDefinition(
                definition.Description,
                name,
                definition.Interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithInterfaces(this InterfaceDefinition definition,
            IReadOnlyCollection<NamedType>? interfaces)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                interfaces,
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithDirectives(this InterfaceDefinition definition,
            IReadOnlyCollection<Directive>? directives)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithFields(this InterfaceDefinition definition,
            IReadOnlyCollection<FieldDefinition>? fields)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                fields,
                definition.Location);
        }
    }
}