using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

/// <summary>
/// Context for directive processing during field collection
/// </summary>
public record DirectiveContext
{
    public required ISchema Schema { get; init; }
    public required ObjectDefinition ObjectDefinition { get; init; }
    public required ISelection Selection { get; init; }
    public required Directive Directive { get; init; }
    public IReadOnlyDictionary<string, object?>? CoercedVariableValues { get; init; }
}

/// <summary>
/// Result of directive processing
/// </summary>
public record DirectiveResult
{
    /// <summary>
    /// Whether to include this selection in the result
    /// </summary>
    public bool Include { get; init; } = true;

    /// <summary>
    /// Whether this directive was handled by this handler
    /// </summary>
    public bool Handled { get; init; } = true;

    /// <summary>
    /// Additional metadata to attach to the selection (for @defer/@stream)
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Handler for processing GraphQL directives during field collection
/// </summary>
public interface IDirectiveHandler
{
    /// <summary>
    /// Process a directive on a selection
    /// </summary>
    /// <param name="context">Directive processing context</param>
    /// <returns>Result indicating how to handle the selection</returns>
    DirectiveResult Handle(DirectiveContext context);
}