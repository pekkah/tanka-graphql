using System.Collections.Generic;

using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class EnumDefinitionExtensions
{
    public static EnumDefinition WithDescription(this EnumDefinition definition,
        in StringValue? description)
    {
        return new EnumDefinition(
            description,
            definition.Name,
            definition.Directives,
            definition.Values,
            definition.Location);
    }

    public static EnumDefinition WithName(this EnumDefinition definition,
        in Name name)
    {
        return new EnumDefinition(
            definition.Description,
            name,
            definition.Directives,
            definition.Values,
            definition.Location);
    }

    public static EnumDefinition WithDirectives(this EnumDefinition definition,
        IReadOnlyList<Directive>? directives)
    {
        return new EnumDefinition(
            definition.Description,
            definition.Name,
            directives != null ? new Directives(directives) : null,
            definition.Values,
            definition.Location);
    }

    public static EnumDefinition WithValues(this EnumDefinition definition,
        IReadOnlyList<EnumValueDefinition>? values)
    {
        return new EnumDefinition(
            definition.Description,
            definition.Name,
            definition.Directives,
            values != null ? new EnumValuesDefinition(values) : null,
            definition.Location);
    }
}