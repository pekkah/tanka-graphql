using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public static class DirectiveExtensions
{
    public static bool TryGetArgument(
        this Directive definition,
        Name argumentName,
        [NotNullWhen(true)] out Argument? argument)
    {
        if (definition.Arguments is null)
        {
            argument = null;
            return false;
        }

        return definition.Arguments.TryGet(argumentName, out argument);
    }

    public static Directive WithName(this Directive directive,
        in Name name)
    {
        return new Directive(
            name,
            directive.Arguments,
            directive.Location);
    }

    public static Directive WithArguments(this Directive directive,
        IReadOnlyList<Argument> arguments)
    {
        return new Directive(
            directive.Name,
            new Arguments(arguments),
            directive.Location);
    }
}