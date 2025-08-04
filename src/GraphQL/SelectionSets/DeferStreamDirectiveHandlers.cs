using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Handler for the @defer directive for incremental delivery
/// </summary>
public class DeferDirectiveHandler : IDirectiveHandler
{
    public DirectiveResult Handle(DirectiveContext context)
    {
        // Check for 'if' argument first
        if (context.Directive.Arguments != null)
        {
            var ifArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "if");
            if (ifArgument != null)
            {
                bool ifValue = GetIfArgumentValue(context.Directive, context.CoercedVariableValues, ifArgument);
                if (!ifValue)
                {
                    // If 'if' is false, don't defer - execute normally
                    return new DirectiveResult { Include = true };
                }
            }
        }

        // Store the directive using its name as the key
        var metadata = new Dictionary<string, object>
        {
            [context.Directive.Name.Value] = context.Directive
        };

        return new DirectiveResult
        {
            Include = true,
            Metadata = metadata
        };
    }

    private static bool GetIfArgumentValue(
        Directive directive,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        Argument argument)
    {
        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }
}

/// <summary>
/// Handler for the @stream directive for incremental delivery
/// </summary>
public class StreamDirectiveHandler : IDirectiveHandler
{
    public DirectiveResult Handle(DirectiveContext context)
    {
        // Check for 'if' argument first
        if (context.Directive.Arguments != null)
        {
            var ifArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "if");
            if (ifArgument != null)
            {
                bool ifValue = GetIfArgumentValue(context.Directive, context.CoercedVariableValues, ifArgument);
                if (!ifValue)
                {
                    // If 'if' is false, don't stream - execute normally
                    return new DirectiveResult { Include = true };
                }
            }
        }

        // Store the directive using its name as the key
        var metadata = new Dictionary<string, object>
        {
            [context.Directive.Name.Value] = context.Directive
        };

        return new DirectiveResult
        {
            Include = true,
            Metadata = metadata
        };
    }

    private static bool GetIfArgumentValue(
        Directive directive,
        IReadOnlyDictionary<string, object?>? coercedVariableValues,
        Argument argument)
    {
        return Ast.GetIfArgumentValue(directive, coercedVariableValues, argument);
    }
}