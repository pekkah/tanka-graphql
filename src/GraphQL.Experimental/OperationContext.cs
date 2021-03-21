using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public class OperationContext
    {
        public OperationContext(OperationDefinition operation,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            OperationValidationResult validationResult,
            OperationExecutor operationExecutor)
        {
            Operation = operation;
            CoercedVariableValues = coercedVariableValues;
            ValidationResult = validationResult;
            OperationExecutor = operationExecutor;
        }

        public OperationDefinition Operation { get; }

        public IReadOnlyDictionary<string, object> CoercedVariableValues { get; }

        public OperationValidationResult ValidationResult { get; }

        public OperationExecutor OperationExecutor { get; }
    }
}