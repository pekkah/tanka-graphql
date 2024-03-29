﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class DirectiveDefinitionExtensions
{
    public static bool TryGetArgument(
        this DirectiveDefinition definition,
        Name argumentName,
        [NotNullWhen(true)] out InputValueDefinition? argument)
    {
        if (definition.Arguments is null)
        {
            argument = null;
            return false;
        }

        return definition.Arguments.TryGet(argumentName, out argument);
    }

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
        IReadOnlyList<InputValueDefinition> arguments)
    {
        return new DirectiveDefinition(
            definition.Description,
            definition.Name,
            new ArgumentsDefinition(arguments),
            definition.IsRepeatable,
            definition.DirectiveLocations,
            definition.Location);
    }

    public static DirectiveDefinition WithDirectiveLocations(this DirectiveDefinition definition,
        IReadOnlyList<string> directiveLocations)
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