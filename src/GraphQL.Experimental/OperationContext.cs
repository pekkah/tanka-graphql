using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public class OperationContext
    {
        public OperationContext(
            ExecutableSchema schema,
            ExecutableDocument document,
            OperationDefinition operation,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            OperationValidationResult validationResult,
            OperationExecutor operationExecutor)
        {
            Schema = schema;
            Document = document;
            Operation = operation;
            CoercedVariableValues = coercedVariableValues;
            ValidationResult = validationResult;
            OperationExecutor = operationExecutor;
        }

        public OperationDefinition Operation { get; }

        public IReadOnlyDictionary<string, object?> CoercedVariableValues { get; }

        public OperationValidationResult ValidationResult { get; }

        public OperationExecutor OperationExecutor { get; }

        public ExecutableSchema Schema { get; }

        public ExecutableDocument Document { get; }
    }
}