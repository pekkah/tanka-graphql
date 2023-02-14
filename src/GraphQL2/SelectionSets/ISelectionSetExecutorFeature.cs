using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public interface ISelectionSetExecutorFeature
{
    Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext queryContext,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path);
}