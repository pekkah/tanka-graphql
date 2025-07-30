using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public class SelectionSetDelegateExecutorFeature : ISelectionSetExecutorFeature
{
    private readonly SelectionSetDelegate _pipeline;

    public SelectionSetDelegateExecutorFeature(SelectionSetDelegate pipeline)
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