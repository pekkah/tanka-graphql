namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public class SchemaValidationResult
{
    public SchemaValidationResult()
    {
        Errors = new List<SchemaValidationError>();
    }

    public SchemaValidationResult(IEnumerable<SchemaValidationError> errors)
    {
        Errors = errors.ToList();
    }

    public IReadOnlyList<SchemaValidationError> Errors { get; }

    public bool IsValid => !Errors.Any();

    public void AddError(SchemaValidationError error)
    {
        if (Errors is List<SchemaValidationError> mutableErrors)
        {
            mutableErrors.Add(error);
        }
        else
        {
            throw new InvalidOperationException("Cannot add error to read-only result");
        }
    }

    public void AddError(string code, string message)
    {
        AddError(new SchemaValidationError(code, message));
    }
}