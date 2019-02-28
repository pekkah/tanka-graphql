using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public static class Validator
    {
        public static ValidationResult Validate(
            IEnumerable<CombineRule> rules,
            ISchema schema,
            GraphQLDocument document,
            Dictionary<string, object> variableValues = null)
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