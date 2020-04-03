using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class DirectiveDefinitionExtensions
    {
        public static DirectiveDefinition WithDescription(this DirectiveDefinition definition,
            in StringValue? description)
        {
            return new DirectiveDefinition(
                description,
                definition.Name,
                definition.Arguments,
                definition.IsRepeatable,
                definition.DirectiveLocations,
                definition.Location);
        }

        public static DirectiveDefinition WithName(this DirectiveDefinition definition,
            in Name name)
        {
            return new DirectiveDefinition(
                definition.Description,
                name,
                definition.Arguments,
                definition.IsRepeatable,
                definition.DirectiveLocations,
                definition.Location);
        }

        public static DirectiveDefinition WithArguments(this DirectiveDefinition definition,
            IReadOnlyCollection<InputValueDefinition> arguments)
        {
            return new DirectiveDefinition(
                definition.Description,
                definition.Name,
                arguments,
                definition.IsRepeatable,
                definition.DirectiveLocations,
                definition.Location);
        }

        public static DirectiveDefinition WithDirectiveLocations(this DirectiveDefinition definition,
            IReadOnlyCollection<string> directiveLocations)
        {
            return new DirectiveDefinition(
                definition.Description,
                definition.Name,
                definition.Arguments,
                definition.IsRepeatable,
                directiveLocations,
                definition.Location);
        }
    }
}