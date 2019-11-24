using System;

namespace Tanka.GraphQL.Validation
{
    public class ValidationException : Exception
    {
        public ValidationResult Result { get; }

        public ValidationException(ValidationResult result)
        {
            Result = result;
        }
    }
}