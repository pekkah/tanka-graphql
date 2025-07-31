using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public class SchemaValidator : ISchemaValidator
{
    private readonly IEnumerable<SchemaValidationRule> _rules;

    public SchemaValidator(IEnumerable<SchemaValidationRule> rules)
    {
        _rules = rules;
    }

    public SchemaValidationResult Validate(IEnumerable<TypeDefinition> typeDefinitions)
    {
        var walker = new SchemaWalker(_rules);
        return walker.Walk(typeDefinitions);
    }
}