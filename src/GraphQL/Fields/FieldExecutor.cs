using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Fields;

public class FieldExecutor : IFieldExecutor
{
    public async Task<object?> Execute(
        QueryContext context,
        ObjectDefinition objectDefinition,
        object? objectValue,
        IReadOnlyCollection<FieldSelection> fields,
        NodePath path)
    {
        var schema = context.Schema;
        var fieldSelection = fields.First();
        var fieldName = fieldSelection.Name;
        path.Append(fieldName);

        // __typename hack
        if (fieldName == "__typename") return objectDefinition.Name.Value;

        var fieldType = schema
            .GetField(objectDefinition.Name, fieldName)?
            .Type;

        if (fieldType == null)
            throw new QueryException(
                $"Object '{objectDefinition.Name}' does not have field '{fieldName}'")
            {
                Path = path
            };

        var field = schema.GetField(objectDefinition.Name, fieldName);
        object? completedValue = null;

        if (field is null)
            return null;

        var argumentValues = ArgumentCoercion.CoerceArgumentValues(
            schema,
            objectDefinition,
            fieldSelection,
            context.CoercedVariableValues);

        try
        {
            var resolver = schema.GetResolver(objectDefinition.Name, fieldName);

            if (resolver == null)
                throw new QueryException(
                    $"Could not get resolver for {objectDefinition.Name}.{fieldName}")
                {
                    Path = path
                };

            var resolverContext = new ResolverContext
            {
                Arguments = argumentValues,
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