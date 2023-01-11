using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Experimental;

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
    public async Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        var groupedFieldSet = SelectionSets.CollectFields(
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
                    path);

                responseMap[responseKey] = completedValue;
            }
            catch (FieldException e)
            {
                responseMap[responseKey] = null;
                context.ErrorCollector.Add(e);
            }

        return responseMap;
    }
}