using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class InputValueDefinitionExtensions
    {
        public static InputValueDefinition WithDescription(this InputValueDefinition definition,
            in StringValue? description)
        {
            return new InputValueDefinition(
                description,
                definition.Name,
                definition.Type,
                definition.DefaultValue,
                definition.Directives,
                definition.Location);
        }

        public static InputValueDefinition WithName(this InputValueDefinition definition, in Name name)
        {
            return new InputValueDefinition(
                definition.Description,
                name,
                definition.Type,
                definition.DefaultValue,
                definition.Directives,
                definition.Location);
        }

        public static InputValueDefinition WithType(this InputValueDefinition definition, Type type)
        {
            return new InputValueDefinition(
                definition.Description,
                definition.Name,
                type,
                definition.DefaultValue,
                definition.Directives,
                definition.Location);
        }

        public static InputValueDefinition WithDirectives(this InputValueDefinition definition,
            IReadOnlyCollection<Directive>? directives)
        {
            return new InputValueDefinition(
                definition.Description,
                definition.Name,
                definition.Type,
                definition.DefaultValue,
                directives,
                definition.Location);
        }
    }
}