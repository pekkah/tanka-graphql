using System.Collections.Generic;

namespace Tanka.GraphQL.Experimental
{
    public class OperationValidationResult
    {
        private readonly List<FieldError> _errors = new List<FieldError>();

        public static OperationValidationResult Success { get; } = new ();

        public IEnumerable<FieldError> Errors => _errors.AsReadOnly();

        public bool HasErrors => _errors.Count > 0;
    }
}