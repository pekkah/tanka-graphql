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
    }
}