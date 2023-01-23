using System.Collections.Immutable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public class SelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    private readonly SelectionSetDelegate _pipeline;

    public SelectionSetExecutorFeature(SelectionSetDelegate pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext queryContext, 
        SelectionSet selectionSet, 
        ObjectDefinition objectType,
        object? objectValue, 
        NodePath path)
    {
        var context = new SelectionSetContext()
        {
            QueryContext = queryContext,
            SelectionSet = selectionSet,
            ObjectDefinition = objectType,
            ObjectValue = objectValue,
            Path = path
        };

        await _pipeline(context);
        return context.Result;
    }
}

public record SelectionSetContext
{
    public required QueryContext QueryContext { get; set; }

    public required SelectionSet SelectionSet { get; set; }

    public required ObjectDefinition ObjectDefinition { get; set; }

    public required object? ObjectValue { get; set; }

    public required NodePath Path { get; set; }

    public IReadOnlyDictionary<string, object?> Result { get; set; } = ImmutableDictionary<string, object?>.Empty;
}

public delegate Task SelectionSetDelegate(SelectionSetContext context);