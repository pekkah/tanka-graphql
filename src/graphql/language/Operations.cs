using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.error;

namespace tanka.graphql.language
{
    public static class Operations
    {
        public static GraphQLOperationDefinition GetOperation(GraphQLDocument document, string operationName)
        {
            var operations = document.Definitions.OfType<GraphQLOperationDefinition>().ToList();

            if (string.IsNullOrEmpty(operationName))
            {
                if (operations.Count == 1)
                {
                    return operations.Single();
                }

                throw new GraphQLError("Multiple operations found. Please provide OperationName");
            }

            var operation = operations.SingleOrDefault(op => op.Name.Value == operationName);

            if (operation == null)
            {
                throw new GraphQLError($"Could not find operation with name {operationName}");
            }

            return operation;
        }
    }
}