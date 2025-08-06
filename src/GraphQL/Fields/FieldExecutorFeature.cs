using System.Collections;

using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Fields;

public class FieldExecutorFeature : IFieldExecutorFeature
{
    public async Task<object?> Execute(
        QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path,
        IReadOnlyDictionary<string, object>? fieldMetadata = null)
    {
        context.RequestCancelled.ThrowIfCancellationRequested();

        ISchema schema = context.Schema;
        FieldSelection fieldSelection = fields.First();
        Name fieldName = fieldSelection.Name;
        path.Append(fieldName);

        // __typename hack
        if (fieldName == "__typename") return objectDefinition.Name.Value;

        FieldDefinition? field = schema.GetField(objectDefinition.Name, fieldName);

        if (field is null)
            return null;

        TypeBase fieldType = field.Type;

        if (fieldType == null)
            throw new QueryException(
                $"Object '{objectDefinition.Name}' does not have field '{fieldName}'")
            {
                Path = path
            };
        object? completedValue = null;

        IReadOnlyDictionary<string, object?> argumentValues = ArgumentCoercion.CoerceArgumentValues(
            schema,
            objectDefinition,
            fieldSelection,
            context.CoercedVariableValues);

        try
        {
            Resolver? resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}")
                {
                    Path = path
                };

            var resolverContext = new ResolverContext
            {
                ArgumentValues = argumentValues,
                Field = field,
                Fields = fields,
                ObjectDefinition = objectDefinition,
                ObjectValue = objectValue,
                Path = path,
                Selection = fieldSelection,
                QueryContext = context,
                FieldMetadata = fieldMetadata != null ? new Dictionary<string, object>(fieldMetadata) : null
            };

            await resolver(resolverContext);

            // Check if this field has @stream directive and pass initialCount to value completion
            if (fieldMetadata?.ContainsKey("stream") == true)
            {
                var streamDirective = (Directive)fieldMetadata["stream"];
                var initialCount = GetDirectiveArgumentValue(streamDirective, "initialCount", context.CoercedVariableValues) as int? ?? 0;
                var label = GetDirectiveArgumentValue(streamDirective, "label", context.CoercedVariableValues) as string;

                await context.CompleteValueAsync(resolverContext, fieldType, path, initialCount, label);
            }
            else
            {
                await context.CompleteValueAsync(resolverContext, fieldType, path);
            }

            return resolverContext.CompletedValue;
        }
        catch (Exception e)
        {
            return e.Handle(
                context,
                objectDefinition,
                fieldName,
                fieldType,
                fieldSelection,
                completedValue,
                path);
        }
    }


    private static object? GetDirectiveArgumentValue(Directive directive, string argumentName, IReadOnlyDictionary<string, object?>? coercedVariableValues)
    {
        var argument = directive.Arguments?.FirstOrDefault(a => a.Name.Value == argumentName);
        if (argument is null) return null;

        return argument.Value switch
        {
            { Kind: NodeKind.StringValue } => ((StringValue)argument.Value).ToString(),
            { Kind: NodeKind.IntValue } => ((IntValue)argument.Value).Value,
            { Kind: NodeKind.BooleanValue } => ((BooleanValue)argument.Value).Value,
            { Kind: NodeKind.Variable } => coercedVariableValues?[((Variable)argument.Value).Name],
            _ => null
        };
    }
}