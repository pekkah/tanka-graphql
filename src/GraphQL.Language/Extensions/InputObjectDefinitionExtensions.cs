using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class InputObjectDefinitionExtensions
{
    public static InputObjectDefinition WithDescription(this InputObjectDefinition definition,
        in StringValue? description)
    {
        return new InputObjectDefinition(
            description,
            definition.Name,
            definition.Directives,
            definition.Fields,
            definition.Location);
    }

    public static InputObjectDefinition WithName(this InputObjectDefinition definition,
        in Name name)
    {
        return new InputObjectDefinition(
            definition.Description,
            name,
            definition.Directives,
            definition.Fields,
            definition.Location);
    }

    public static InputObjectDefinition WithDirectives(this InputObjectDefinition definition,
        IReadOnlyList<Directive>? directives)
    {
        return new InputObjectDefinition(
            definition.Description,
            definition.Name,
            Directives.From(directives),
            definition.Fields,
            definition.Location);
    }

    public static InputObjectDefinition WithFields(this InputObjectDefinition definition,
        IReadOnlyList<InputValueDefinition>? fields)
    {
        return new InputObjectDefinition(
            definition.Description,
            definition.Name,
            definition.Directives,
            InputFieldsDefinition.From(fields),
            definition.Location);
    }
}