namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public class SchemaValidationException : Exception
{
    public SchemaValidationException(IEnumerable<SchemaValidationError> errors)
        : base($"Schema validation failed with {errors.Count()} error(s)")
    {
        Errors = errors.ToList();
    }

    public SchemaValidationException(string message, IEnumerable<SchemaValidationError> errors)
        : base(message)
    {
        Errors = errors.ToList();
    }

    public IReadOnlyList<SchemaValidationError> Errors { get; }

    public override string ToString()
    {
        var errorMessages = string.Join("\n", Errors.Select(e => $"  - {e}"));
        return $"{Message}\n{errorMessages}";
    }
}