﻿using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public class ValidatorFeature : IValidatorFeature
{
    public IValidator3? Validator { get; set; }

    public ValueTask<ValidationResult> Validate(
        ISchema schema,
        ExecutableDocument document,
        IReadOnlyDictionary<string, object?>? variables)
    {
        ArgumentNullException.ThrowIfNull(Validator);

        return Validator.Validate(
            schema,
            document,
            variables);
    }
}