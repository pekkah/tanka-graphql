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
        NodePath path)
    {
        context.RequestCancelled.ThrowIfCancellationRequested();
        
        ISchema schema = context.Schema;
        FieldSelection fieldSelection = fields.First();
        Name fieldName = fieldSelection.Name;
        path.Append(fieldName);

        // __typename hack
        if (fieldName == "__typename") return objectDefinition.Name.Value;

        TypeBase? fieldType = schema
            .GetField(objectDefinition.Name, fieldName)?
            .Type;

        if (fieldType == null)
            throw new QueryException(
                $"Object '{objectDefinition.Name}' does not have field '{fieldName}'")
            {
                Path = path
            };

        FieldDefinition? field = schema.GetField(objectDefinition.Name, fieldName);
        object? completedValue = null;

        if (field is null)
            return null;

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
                QueryContext = context
            };

            await resolver(resolverContext);
            await context.CompleteValueAsync(resolverContext, fieldType, path);

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
}