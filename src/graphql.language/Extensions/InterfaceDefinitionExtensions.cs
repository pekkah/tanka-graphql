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
            IReadOnlyList<NamedType>? interfaces)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                ImplementsInterfaces.From(interfaces),
                definition.Directives,
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithDirectives(this InterfaceDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                Directives.From(directives), 
                definition.Fields,
                definition.Location);
        }

        public static InterfaceDefinition WithFields(this InterfaceDefinition definition,
            IReadOnlyList<FieldDefinition>? fields)
        {
            return new InterfaceDefinition(
                definition.Description,
                definition.Name,
                definition.Interfaces,
                definition.Directives,
                FieldsDefinition.From(fields),
                definition.Location);
        }
    }
}