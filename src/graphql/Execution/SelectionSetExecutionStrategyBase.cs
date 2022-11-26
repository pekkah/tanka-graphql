using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution;

public abstract class SelectionSetExecutionStrategyBase : IExecutionStrategy
{
    public virtual async Task<IDictionary<string, object?>?> ExecuteSelectionSetAsync(
        IExecutorContext executorContext,
        SelectionSet selectionSet,
        ObjectDefinition objectDefinition,
        object objectValue,
        NodePath path)
    {
        if (executorContext == null) throw new ArgumentNullException(nameof(executorContext));
        if (selectionSet == null) throw new ArgumentNullException(nameof(selectionSet));
        if (path == null) throw new ArgumentNullException(nameof(path));

        var groupedFieldSet = CollectFields(
            executorContext.Schema,
            executorContext.Document,
            objectDefinition,
            selectionSet,
            executorContext.CoercedVariableValues);

        var resultMap = await ExecuteGroupedFieldSetAsync(
            executorContext,
            groupedFieldSet,
            objectDefinition,
            objectValue,
            path).ConfigureAwait(false);

        return resultMap;
    }

    protected abstract Task<IDictionary<string, object?>> ExecuteGroupedFieldSetAsync(
        IExecutorContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectDefinition,
        object? objectValue,
        NodePath path);

    protected virtual IReadOnlyDictionary<string, List<FieldSelection>> CollectFields(
        ISchema schema,
        ExecutableDocument document,
        ObjectDefinition objectDefinition,
        SelectionSet selectionSet,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        List<string>? visitedFragments = null)
    {
        return SelectionSets.CollectFields(
            schema,
            document,
            objectDefinition,
            selectionSet,
            coercedVariableValues,
            visitedFragments);
    }
}