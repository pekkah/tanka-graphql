using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Language;

public static class EnumValueDefinitionExtensions
{
    public static bool IsDeprecated(
        this EnumValueDefinition definition,
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
        this EnumValueDefinition definition,
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