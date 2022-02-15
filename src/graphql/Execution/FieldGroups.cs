using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Execution;

public static class FieldGroups
{
    public static async Task<object?> ExecuteFieldAsync(
        IExecutorContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        TypeBase fieldType,
        NodePath path)
    {
        if (fields == null) throw new ArgumentNullException(nameof(fields));
        if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));

        var schema = context.Schema;
        var fieldSelection = fields.First();
        var fieldName = fieldSelection.Name;
        var field = schema.GetField(objectDefinition.Name, fieldName);
        object? completedValue = null;

        if (field is null)
            return completedValue;

        var argumentValues = ArgumentCoercion.CoerceArgumentValues(
            schema,
            objectDefinition,
            fieldSelection,
            context.CoercedVariableValues);

        try
        {
            var resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryExecutionException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}",
                    path);

            var resolverContext =
                new ResolverContext(
                    objectDefinition,
                    objectValue,
                    field,
                    fieldSelection,
                    fields,
                    argumentValues,
                    path,
                    context);

            // begin resolve
            await context.ExtensionsRunner.BeginResolveAsync(resolverContext);
            var resultTask = resolver(resolverContext);

            IResolverResult result;
            if (resultTask.IsCompletedSuccessfully)
                result = resultTask.Result;
            else
                result = await resultTask;

            await context.ExtensionsRunner.EndResolveAsync(resolverContext, result);
            // end resolve

            // begin complete
            var completedValueTask = result.CompleteValueAsync(resolverContext);
            if (completedValueTask.IsCompletedSuccessfully)
                completedValue = completedValueTask.Result;
            else
                completedValue = await completedValueTask;
            // end complete

            return completedValue;
        }
        catch (Exception e)
        {
            return FieldErrors.Handle(
                context,
                objectDefinition,
                fieldName,
                fieldType,
                fieldSelection,
                completedValue,
                e,
                path);
        }
    }

    public static async Task<object?> ExecuteFieldGroupAsync(
        IExecutorContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        KeyValuePair<string, IReadOnlyCollection<FieldSelection>> fieldGroup,
        NodePath path)
    {
        var schema = context.Schema;
        var fields = fieldGroup.Value;
        var fieldName = fields.First().Name;
        path.Append(fieldName);

        // __typename hack
        if (fieldName == "__typename") return objectDefinition.Name.Value;

        var fieldType = schema
            .GetField(objectDefinition.Name, fieldName)?
            .Type;

        if (fieldType == null)
            throw new QueryExecutionException(
                $"Object '{objectDefinition.Name}' does not have field '{fieldName}'",
                path);

        var responseValue = await ExecuteFieldAsync(
            context,
            objectDefinition,
            objectValue,
            fields,
            fieldType,
            path).ConfigureAwait(false);

        return responseValue;
    }
}