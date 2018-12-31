using System;

namespace tanka.graphql.validation
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