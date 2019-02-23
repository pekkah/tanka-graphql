using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each named spread namedSpread in the document
    ///     Let fragment be the target of namedSpread
    ///     The target type of fragment must be defined in the schema
    /// </summary>
    public class R5512FragmentSpreadTypeExistence : TypeTrackingRuleBase
    {
        public override void BeginVisitFragmentDefinition(GraphQLFragmentDefinition node, IValidationContext context)
        {
            base.BeginVisitFragmentDefinition(node, context);

            var type = GetCurrentType();

            if (type == null)
                context.Error(
                    ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                    "Fragments must be specified on types that exist in the schema. This " +
                    "applies for both named and inline fragments. ",
                    node);
        }

        public override void BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment, IValidationContext context)
        {
            base.BeginVisitInlineFragment(inlineFragment, context);

            var type = GetCurrentType();

            if (type == null)
                context.Error(
                    ValidationErrorCodes.R5512FragmentSpreadTypeExistence,
                    "Fragments must be specified on types that exist in the schema. This " +
                    "applies for both named and inline fragments. ",
                    inlineFragment);
        }
    }
}