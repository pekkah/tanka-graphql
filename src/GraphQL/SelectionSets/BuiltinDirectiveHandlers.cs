using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Handler for the built-in @skip directive
/// </summary>
public class SkipDirectiveHandler : IDirectiveHandler
{
    public DirectiveResult Handle(DirectiveContext context)
    {
        if (context.Directive.Name.Value != "skip")
            return new DirectiveResult { Handled = false };

        if (context.Directive.Arguments == null)
            return new DirectiveResult { Include = true };

        var ifArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "if");
        bool shouldSkip = GetIfArgumentValue(context.Directive, context.CoercedVariableValues, ifArgument);

        return new DirectiveResult { Include = !shouldSkip };
    }

    private static bool GetIfArgumentValue(
        Directive directive,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        Argument? argument)
    {
        if (argument is null)
            return false;

        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }
}

/// <summary>
/// Handler for the built-in @include directive
/// </summary>
public class IncludeDirectiveHandler : IDirectiveHandler
{
    public DirectiveResult Handle(DirectiveContext context)
    {
        if (context.Directive.Name.Value != "include")
            return new DirectiveResult { Handled = false };

        if (context.Directive.Arguments == null)
            return new DirectiveResult { Include = true };

        var ifArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "if");
        bool shouldInclude = GetIfArgumentValue(context.Directive, context.CoercedVariableValues, ifArgument);

        return new DirectiveResult { Include = shouldInclude };
    }

    private static bool GetIfArgumentValue(
        Directive directive,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        Argument? argument)
    {
        if (argument is null)
            return false;

        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }
}