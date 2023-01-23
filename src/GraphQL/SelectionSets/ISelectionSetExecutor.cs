using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public interface ISelectionSetExecutor
{
    static ISelectionSetExecutor Default = new SelectionSetExecutor();

    Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path);
}

public class SelectionSetExecutor : ISelectionSetExecutor
{
    public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        return context.OperationDefinition.Operation switch
        {
            OperationType.Query => ExecuteParallel(context, selectionSet, objectType, objectValue, path),
            OperationType.Mutation => ExecuteSerial(context, selectionSet, objectType, objectValue, path),
            OperationType.Subscription => ExecuteParallel(context, selectionSet, objectType, objectValue, path),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static async Task<IReadOnlyDictionary<string, object?>> ExecuteSerial(QueryContext context,
        SelectionSet selectionSet, ObjectDefinition objectType, object? objectValue, NodePath path)
    {
        var groupedFieldSet = SelectionSetExtensions.CollectFields(
            context.Schema,
            context.Request.Document,
            objectType,
            selectionSet,
            context.CoercedVariableValues);

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
                context.ErrorCollector.Add(e);
            }

        return responseMap;
    }

    private static async Task<IReadOnlyDictionary<string, object?>> ExecuteParallel(QueryContext context,
        SelectionSet selectionSet, ObjectDefinition objectType, object? objectValue, NodePath path)
    {
        var groupedFieldSet = SelectionSetExtensions.CollectFields(
            context.Schema,
            context.Request.Document,
            objectType,
            selectionSet,
            context.CoercedVariableValues);

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