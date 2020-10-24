using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language
{
    public static class EnumValueDefinitionExtensions
    {
        public static EnumValueDefinition WithDescription(this EnumValueDefinition definition,
            in StringValue? description)
        {
            return new EnumValueDefinition(
                description,
                definition.Value,
                definition.Directives,
                definition.Location);
        }

        public static EnumValueDefinition WithValue(this EnumValueDefinition definition,
            in EnumValue value)
        {
            return new EnumValueDefinition(
                definition.Description,
                value,
                definition.Directives,
                definition.Location);
        }

        public static EnumValueDefinition WithDirectives(this EnumValueDefinition definition,
            IReadOnlyList<Directive>? directives)
        {
            return new EnumValueDefinition(
                definition.Description,
                definition.Value,
                directives != null ? new Directives(directives) : null,
                definition.Location);
        }
    }
}