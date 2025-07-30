using Tanka.GraphQL.TypeSystem.SchemaValidation.Rules;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public static class BuiltInSchemaValidationRules
{
    public static IEnumerable<SchemaValidationRule> All => new SchemaValidationRule[]
    {
        new OneOfInputObjectRule()
    };
}