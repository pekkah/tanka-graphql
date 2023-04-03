using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class ScalarDefinitionExtensions
{
    public static ScalarDefinition WithDescription(this ScalarDefinition definition,
        in StringValue? description)
    {
        return new ScalarDefinition(
            description,
            definition.Name,
            definition.Directives,
            definition.Location);
    }

    public static ScalarDefinition WithName(this ScalarDefinition definition,
        in Name name)
    {
        return new ScalarDefinition(
            definition.Description,
            name,
            definition.Directives,
            definition.Location);
    }

    public static ScalarDefinition WithDirectives(this ScalarDefinition definition,
        IReadOnlyList<Directive>? directives)
    {
        return new ScalarDefinition(
            definition.Description,
            definition.Name,
            Directives.From(directives),
            definition.Location);
    }
}