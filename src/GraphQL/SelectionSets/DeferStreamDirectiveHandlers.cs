using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Handler for the @defer directive for incremental delivery
/// </summary>
public class DeferDirectiveHandler : IDirectiveHandler
{
    public DirectiveResult Handle(DirectiveContext context)
    {
        if (context.Directive.Name.Value != "defer")
            return new DirectiveResult { Handled = false };

        // For now, always include the selection (defer processing happens later in execution)
        // The actual deferred execution logic will be implemented in the execution pipeline
        var metadata = new Dictionary<string, object>
        {
            ["deferred"] = true
        };

        // Check for 'if' argument
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

            // Check for 'label' argument
            var labelArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "label");
            if (labelArgument != null && labelArgument.Value is StringValue labelValue)
            {
                metadata["label"] = labelValue.Value;
            }
        }

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
        if (context.Directive.Name.Value != "stream")
            return new DirectiveResult { Handled = false };

        // For now, always include the selection (stream processing happens later in execution)
        // The actual streaming execution logic will be implemented in the execution pipeline
        var metadata = new Dictionary<string, object>
        {
            ["streamed"] = true
        };

        // Check for 'if' argument
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

            // Check for 'label' argument
            var labelArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "label");
            if (labelArgument != null && labelArgument.Value is StringValue labelValue)
            {
                metadata["label"] = labelValue.Value;
            }

            // Check for 'initialCount' argument
            var initialCountArgument = context.Directive.Arguments.SingleOrDefault(a => a.Name == "initialCount");
            if (initialCountArgument != null && initialCountArgument.Value is IntValue initialCountValue)
            {
                metadata["initialCount"] = initialCountValue.Value;
            }
        }

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