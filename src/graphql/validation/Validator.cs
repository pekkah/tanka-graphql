using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using tanka.graphql.type;
using tanka.graphql.validation.rules;

namespace tanka.graphql.validation
{
    public static class Validator
    {
        public static ValidationResult Validate(
            IEnumerable<IRule> rules,
            ISchema schema,
            GraphQLDocument document,
            Dictionary<string, object> variableValues = null)
        {
            var visitor = new DocumentRulesVisitor(rules, schema, document, variableValues);
            return visitor.Validate();
        }

        public static async Task<ValidationResult> ValidateAsync(
            ISchema schema,
            GraphQLDocument document,
            IDictionary<string, object> variables = null, 
            IEnumerable<IValidationRule> rules = null)
        {
            var context = new ValidationContext
            {
                Schema = schema,
                Document = document,
                TypeInfo = new TypeInfo(schema),
                Variables = variables ?? new Dictionary<string, object>()
            };

            if (rules == null) rules = CoreRules();

            var visitors = rules.Select(x => x.CreateVisitor(context)).ToList();

            visitors.Insert(0, context.TypeInfo);
// #if DEBUG
//             visitors.Insert(1, new DebugNodeVisitor());
// #endif

            var basic = new BasicVisitor(visitors.ToArray());
            basic.Visit(document);

            var result = new ValidationResult {Errors = context.Errors};
            return result;
        }

        public static List<IValidationRule> CoreRules()
        {
            var rules = new List<IValidationRule>
            {
                new R511ExecutableDefinitions(),
                new UniqueOperationNames(),
                new LoneAnonymousOperation(),
                new KnownTypeNames(),
                new FragmentsOnCompositeTypes(),
                new VariablesAreInputTypes(),
                new ScalarLeafs(),
                new FieldsOnCorrectType(),
                new UniqueFragmentNames(),
                new KnownFragmentNames(),
                new NoUnusedFragments(),
                new PossibleFragmentSpreads(),
                new NoFragmentCycles(),
                new NoUndefinedVariables(),
                new NoUnusedVariables(),
                new UniqueVariableNames(),
                new KnownDirectives(),
                new UniqueDirectivesPerLocation(),
                new KnownArgumentNames(),
                new UniqueArgumentNames(),
                new ArgumentsOfCorrectType(),
                new ProvidedNonNullArguments(),
                new DefaultValuesOfCorrectType(),
                new VariablesInAllowedPosition(),
                new UniqueInputFieldNames(),
                new SubscriptionHasSingleRootField()
            };
            return rules;
        }
    }
}