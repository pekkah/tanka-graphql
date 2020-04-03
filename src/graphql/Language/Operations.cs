using System.Linq;
using Tanka.GraphQL.Language.Nodes;


namespace Tanka.GraphQL.Language
{
    public static class Operations
    {
        public static OperationDefinition GetOperation(ExecutableDocument document, string operationName)
        {
            var operations = document.OperationDefinitions;
            
            if (operations == null)
                throw new DocumentException($"Document does not contain operations");

            if (string.IsNullOrEmpty(operationName))
            {
                if (operations.Count == 1)
                {
                    return operations.Single();
                }

                throw new DocumentException(
                    "Multiple operations found. Please provide OperationName");
            }

            var operation = operations.SingleOrDefault(op => op.Name.Value == operationName);

            if (operation == null)
            {
                throw new DocumentException(
                    $"Could not find operation with name {operationName}");
            }

            return operation;
        }
    }
}