using System;

namespace fugu.graphql.validation
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