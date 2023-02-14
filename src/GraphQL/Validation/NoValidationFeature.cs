using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public class NoValidationFeature : IValidatorFeature
{
    private static ValueTask<ValidationResult> Success = new ValueTask<ValidationResult>(new ValidationResult());

    public ValueTask<ValidationResult> Validate(ISchema schema, ExecutableDocument document, IReadOnlyDictionary<string, object?>? variables)
    {
        return Success;
    }
}