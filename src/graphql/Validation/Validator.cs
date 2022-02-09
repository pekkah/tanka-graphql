using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public static class Validator
    {
        public static ValidationResult Validate(
            IEnumerable<CombineRule> rules,
            ISchema schema,
            ExecutableDocument document,
            IReadOnlyDictionary<string, object?>? variableValues = null)
        {
            var visitor = new RulesWalker(
                rules,
                schema,
                document,
                variableValues);

            return visitor.Validate();
        }
    }
}