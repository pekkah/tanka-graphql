using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public class SelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    public ISelectionSetExecutor SelectionSetExecutor { get; set; } = ISelectionSetExecutor.Default;

    public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(QueryContext context, SelectionSet selectionSet, ObjectDefinition objectType,
        object? objectValue, NodePath path)
    {
        return SelectionSetExecutor.ExecuteSelectionSet(context, selectionSet, objectType, objectValue, path);
    }
}