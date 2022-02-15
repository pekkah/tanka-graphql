using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class FieldDefinitionExtension
{
    public static bool TryGetArgument(
        this FieldDefinition definition,
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

    public static bool IsDeprecated(
        this FieldDefinition definition,
        out string? reason)
    {
        if (definition.Directives is null)
        {
            reason = null;
            return false;
        }

        if (definition.TryGetDirective("deprecated", out var directive))
        {
            if (directive.Arguments?.TryGet("reason", out var reasonArg) == true &&
                reasonArg.Value is StringValue stringValue)
                reason = stringValue;
            else
                reason = null;

            return true;
        }

        reason = null;
        return false;
    }


    public static bool TryGetDirective(
        this FieldDefinition definition,
        Name directiveName,
        [NotNullWhen(true)] out Directive? directive)
    {
        if (definition.Directives is null)
        {
            directive = null;
            return false;
        }

        return definition.Directives.TryGet(directiveName, out directive);
    }

    public static FieldDefinition WithDescription(this FieldDefinition definition, in StringValue description)
    {
        return new FieldDefinition(
            description,
            definition.Name,
            definition.Arguments,
            definition.Type,
            definition.Directives,
            definition.Location);
    }

    public static FieldDefinition WithName(this FieldDefinition definition, in Name name)
    {
        return new FieldDefinition(
            definition.Description,
            name,
            definition.Arguments,
            definition.Type,
            definition.Directives,
            definition.Location);
    }

    public static FieldDefinition WithArguments(this FieldDefinition definition,
        IReadOnlyList<InputValueDefinition>? arguments)
    {
        return new FieldDefinition(
            definition.Description,
            definition.Name,
            ArgumentsDefinition.From(arguments),
            definition.Type,
            definition.Directives,
            definition.Location);
    }

    public static FieldDefinition WithType(this FieldDefinition definition, TypeBase type)
    {
        return new FieldDefinition(
            definition.Description,
            definition.Name,
            definition.Arguments,
            type,
            definition.Directives,
            definition.Location);
    }

    public static FieldDefinition WithDirectives(this FieldDefinition definition,
        IReadOnlyList<Directive>? directives)
    {
        return new FieldDefinition(
            definition.Description,
            definition.Name,
            definition.Arguments,
            definition.Type,
            Directives.From(directives),
            definition.Location);
    }
}