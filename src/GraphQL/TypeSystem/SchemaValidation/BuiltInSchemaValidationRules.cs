using Tanka.GraphQL.TypeSystem.SchemaValidation.Rules;

namespace Tanka.GraphQL.TypeSystem.SchemaValidation;

public static class BuiltInSchemaValidationRules
{
    public static IEnumerable<SchemaValidationRule> BuildAll() => [new OneOfInputObjectRule()];
}