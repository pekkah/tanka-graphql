using Tanka.GraphQL.TypeSystem.SchemaValidation.Rules;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public static class BuiltInSchemaValidationRules
{
    private static readonly SchemaValidationRule[] _allRules = [new OneOfInputObjectRule()];

    public static IEnumerable<SchemaValidationRule> All => _allRules;
}