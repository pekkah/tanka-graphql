namespace Tanka.GraphQL.Validation;

public static class ValidatorQueryContextExtensions
{
    public static ValueTask<ValidationResult> Validate(this QueryContext context)
    {
        var validator = context.ValidatorFeature;

        if (validator is null)
            return new(ValidationResult.Success);

        return validator.Validate(
            context.Schema,
            context.Request.Document,
            context.Request.Variables
        );
    }
}