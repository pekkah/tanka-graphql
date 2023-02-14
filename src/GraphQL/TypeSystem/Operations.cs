using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.TypeSystem;

public static class Operations
{
    public static OperationDefinition GetOperation(ExecutableDocument document, string? operationName)
    {
        var operations = document.OperationDefinitions;

        if (operations == null)
            throw new QueryException("Document does not contain operations")
            {
                Path = new()
            };

        if (string.IsNullOrEmpty(operationName))
        {
            if (operations.Count == 1) return operations.Single();

            throw new QueryException(
                "Multiple operations found. Please provide OperationName")
            {
                Path = new()
            };
        }

        var operation = operations.SingleOrDefault(op => op.Name.Value == operationName);

        if (operation == null)
            throw new QueryException(
                $"Could not find operation with name {operationName}")
            {
                Path = new()
            };

        return operation;
    }
}