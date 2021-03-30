using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Experimental
{
    public class OperationPlanContext
    {
        public OperationDefinition? Operation { get; set; }

        public IReadOnlyDictionary<string, object?>? CoercedVariableValues { get; set; }

        public OperationValidationResult? ValidationResult { get; set; }

        public OperationExecutor? OperationExecutor { get; set; }

        public ExecuteSelectionSet? ExecuteSelectionSet { get; set; }
    }
}