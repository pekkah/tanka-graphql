using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental
{
    public class OperationValidationResult
    {
        private readonly List<FieldException> _errors = new();

        public static OperationValidationResult Success { get; } = new();

        public IEnumerable<FieldException> Errors => _errors.AsReadOnly();

        public bool HasErrors => _errors.Count > 0;
    }
}