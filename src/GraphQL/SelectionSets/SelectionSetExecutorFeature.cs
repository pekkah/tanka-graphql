using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public class DefaultSelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        var groupedFieldSet = FieldCollector.CollectFields(
            context.Schema,
            context.Request.Document,
            objectType,
            selectionSet,
            context.CoercedVariableValues);

        return ExecuteSelectionSet(
            context,
            groupedFieldSet,
            objectType,
            objectValue,
            path);
    }

    public static Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        context.RequestCancelled.ThrowIfCancellationRequested();

        return context.OperationDefinition.Operation switch
        {
            OperationType.Query => ExecuteParallel(context, groupedFieldSet, objectType, objectValue, path),
            OperationType.Mutation => ExecuteSerial(context, groupedFieldSet, objectType, objectValue, path),
            OperationType.Subscription => ExecuteParallel(context, groupedFieldSet, objectType, objectValue, path),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static async Task<IReadOnlyDictionary<string, object?>> ExecuteSerial(
        QueryContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        var responseMap = new Dictionary<string, object?>();

        foreach (var (responseKey, fields) in groupedFieldSet)
            try
            {
                var completedValue = await context.ExecuteField(
                    objectType,
                    objectValue,
                    fields,
                    path.Fork());

                responseMap[responseKey] = completedValue;
            }
            catch (FieldException e)
            {
                responseMap[responseKey] = null;
                context.AddError(e);
            }

        return responseMap;
    }

    public static async Task<IReadOnlyDictionary<string, object?>> ExecuteParallel(QueryContext context,
        IReadOnlyDictionary<string, List<FieldSelection>> groupedFieldSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        var tasks = new Dictionary<string, Task<object?>>();
        foreach (var (responseKey, fields) in groupedFieldSet)
        {
            var fieldPath = path.Fork();
            var executionTask = context.ExecuteField(
                objectType,
                objectValue,
                fields,
                fieldPath);

            tasks.Add(responseKey, executionTask);
        }

        await Task.WhenAll(tasks.Values);
        return tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);
    }
}