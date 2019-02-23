using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each fragment defined in the document.
    ///     The target type of fragment must have kind UNION, INTERFACE, or OBJECT.
    /// </summary>
    public class R5513FragmentsOnCompositeTypes : TypeTrackingRuleBase
    {
        public override void BeginVisitFragmentDefinition(GraphQLFragmentDefinition node, IValidationContext context)
        {
            base.BeginVisitFragmentDefinition(node, context);

            var type = GetCurrentType();

            if (type is UnionType)
                return;

            if (type is ComplexType)
                return;

            context.Error(
                ValidationErrorCodes.R5513FragmentsOnCompositeTypes,
                "Fragments can only be declared on unions, interfaces, and objects",
                node);
        }

        public override void BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment, IValidationContext context)
        {
            base.BeginVisitInlineFragment(inlineFragment, context);

            var type = GetCurrentType();

            if (type is UnionType)
                return;

            if (type is ComplexType)
                return;

            context.Error(
                ValidationErrorCodes.R5513FragmentsOnCompositeTypes,
                "Fragments can only be declared on unions, interfaces, and objects",
                inlineFragment);
        }
    }
}