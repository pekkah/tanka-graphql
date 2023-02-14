using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public interface IValidatorFeature
{
    ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables);
}