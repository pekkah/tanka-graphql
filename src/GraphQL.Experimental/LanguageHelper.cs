using System;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public static class ExecutableDocumentExtensions
    {
        public static OperationDefinition GetOperation(this ExecutableDocument document, string? operationName)
        {
            var operations = document.OperationDefinitions;

            if (operations == null || operations.Count == 0)
                throw new Exception("Document does not contain operations");

            if (string.IsNullOrEmpty(operationName))
            {
                if (operations.Count == 1) return operations.Single();

                throw new Exception(
                    "Multiple operations found. Please provide OperationName");
            }

            var operation = operations.SingleOrDefault(op => op.Name.Value == operationName);

            if (operation == null)
                throw new Exception(
                    $"Could not find operation with name {operationName}");

            return operation;
        }
    }
}