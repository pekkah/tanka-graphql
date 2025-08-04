using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.SelectionSets;

public class DefaultSelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    private readonly IFieldCollector _fieldCollector;

    public DefaultSelectionSetExecutorFeature(IFieldCollector fieldCollector)
    {
        _fieldCollector = fieldCollector;
    }

    public Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        SelectionSet selectionSet,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        var collectionResult = _fieldCollector.CollectFields(
            context.Schema,
            context.Request.Query,
            objectType,
            selectionSet,
            context.CoercedVariableValues);

        return ExecuteSelectionSet(
            context,
            collectionResult,
            objectType,
            objectValue,
            path);
    }

    public static Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSet(
        QueryContext context,
        FieldCollectionResult collectionResult,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path)
    {
        // Check if we have any deferred fields
        if (HasDeferredFields(collectionResult))
        {
            // Get or create the incremental delivery feature
            var incrementalFeature = context.Features.Get<IIncrementalDeliveryFeature>() ??
                                   new DefaultIncrementalDeliveryFeature();
            context.Features.Set<IIncrementalDeliveryFeature>(incrementalFeature);

            return ExecuteSelectionSetWithIncrementalDelivery(
                context,
                collectionResult,
                objectType,
                objectValue,
                path,
                incrementalFeature);
        }

        // No deferred fields, execute normally
        return ExecuteSelectionSet(
            context,
            collectionResult.Fields,
            objectType,
            objectValue,
            path);
    }

    private static bool HasDeferredFields(FieldCollectionResult collectionResult)
    {
        return collectionResult.FieldMetadata?.Values
            .Any(metadata => metadata.ContainsKey("defer") || metadata.ContainsKey("stream")) == true;
    }

    private static async Task<IReadOnlyDictionary<string, object?>> ExecuteSelectionSetWithIncrementalDelivery(
        QueryContext context,
        FieldCollectionResult collectionResult,
        ObjectDefinition objectType,
        object? objectValue,
        NodePath path,
        IIncrementalDeliveryFeature incrementalFeature)
    {
        var responseMap = new Dictionary<string, object?>();

        foreach (var (responseKey, fields) in collectionResult.Fields)
        {
            // Check if this field has incremental delivery directives
            if (collectionResult.FieldMetadata?.TryGetValue(responseKey, out var metadata) == true)
            {
                if (metadata.ContainsKey("defer"))
                {
                    // Register deferred work
                    var deferDirective = (Directive)metadata["defer"];
                    var label = GetDirectiveArgumentValue(deferDirective, "label", context.CoercedVariableValues) as string;

                    incrementalFeature.RegisterDeferredWork(label, path, async () =>
                    {
                        try
                        {
                            var fieldResult = await context.ExecuteField(
                                objectType,
                                objectValue,
                                fields,
                                path.Fork());

                            var data = new Dictionary<string, object?>();
                            if (fieldResult != null)
                                data[responseKey] = fieldResult;

                            return new IncrementalPayload
                            {
                                Label = label,
                                Path = path,
                                Data = data
                            };
                        }
                        catch (Exception ex)
                        {
                            return new IncrementalPayload
                            {
                                Label = label,
                                Path = path,
                                Errors = new[]
                                {
                                    new ExecutionError
                                    {
                                        Message = ex.Message,
                                        Path = path.Append(responseKey).Segments.ToArray()
                                    }
                                }
                            };
                        }
                    });
                }
                else if (metadata.ContainsKey("stream"))
                {
                    // Handle @stream directive - for now execute normally
                    // TODO: Implement proper streaming of list items
                    // @stream requires different handling as it streams individual list items
                    // rather than deferring the entire field
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
                }
                else
                {
                    // No incremental delivery directives, execute normally
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
                }
            }
            else
            {
                // No metadata, execute field immediately
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
            }
        }

        return responseMap;
    }

    private static object? GetDirectiveArgumentValue(Directive directive, string argumentName, IReadOnlyDictionary<string, object?>? coercedVariableValues)
    {
        var argument = directive.Arguments?.FirstOrDefault(a => a.Name.Value == argumentName);
        if (argument is null) return null;

        switch (argument.Value)
        {
            case { Kind: NodeKind.StringValue }:
                return ((StringValue)argument.Value).ToString();
            case { Kind: NodeKind.IntValue }:
                return ((IntValue)argument.Value).Value;
            case { Kind: NodeKind.BooleanValue }:
                return ((BooleanValue)argument.Value).Value;
            case { Kind: NodeKind.Variable }:
                var variable = (Variable)argument.Value;
                var variableValue = coercedVariableValues?[variable.Name];
                return variableValue; // Return the coerced variable value as-is
            default:
                return null;
        }
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