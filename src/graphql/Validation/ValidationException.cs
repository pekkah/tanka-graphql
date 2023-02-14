namespace Tanka.GraphQL.Validation;

public class ValidationException : Exception
{
    public ValidationException(ValidationResult result) : base(result.ToString())
    {
        Result = result;
    }

    public ValidationResult Result { get; }
}