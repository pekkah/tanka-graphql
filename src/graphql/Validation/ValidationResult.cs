using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tanka.GraphQL.Validation;

public class ValidationResult
{
    public IEnumerable<ValidationError> Errors { get; set; } = Array.Empty<ValidationError>();

    public IReadOnlyDictionary<string, object>? Extensions { get; set; }

    public bool IsValid => !Errors.Any();

    public static ValidationResult Success => new();

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"IsValid: {IsValid}");

        if (!IsValid)
        {
            builder.AppendLine("ExecutionErrors:");
            foreach (var validationError in Errors) builder.AppendLine(validationError.ToString());
        }


        return builder.ToString();
    }
}