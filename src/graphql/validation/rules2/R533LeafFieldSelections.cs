using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each selection in the document
    ///     Let selectionType be the result type of selection
    ///     If selectionType is a scalar or enum:
    ///     The subselection set of that selection must be empty
    ///     If selectionType is an interface, union, or object
    ///     The subselection set of that selection must NOT BE empty
    /// </summary>
    public class R533LeafFieldSelections : TypeTrackingRuleBase
    {
        public override void BeginVisitFieldSelection(GraphQLFieldSelection selection,
            IValidationContext context)
        {
            base.BeginVisitFieldSelection(selection, context);

            var fieldName = selection.Name.Value;

            if (fieldName == "__typename")
                return;

            var field = GetFieldDef();

            if (field != null)
            {
                var selectionType = field.Value.Field.Type;
                var hasSubSelection = selection.SelectionSet?.Selections?.Any();

                if (selectionType is ScalarType && hasSubSelection == true)
                    context.Error(
                        ValidationErrorCodes.R533LeafFieldSelections,
                        "Field selections on scalars or enums are never " +
                        "allowed, because they are the leaf nodes of any GraphQL query.",
                        selection);

                if (selectionType is EnumType && hasSubSelection == true)
                    context.Error(
                        ValidationErrorCodes.R533LeafFieldSelections,
                        "Field selections on scalars or enums are never " +
                        "allowed, because they are the leaf nodes of any GraphQL query.",
                        selection);

                if (selectionType is ComplexType && hasSubSelection == null)
                    context.Error(
                        ValidationErrorCodes.R533LeafFieldSelections,
                        "Leaf selections on objects, interfaces, and unions " +
                        "without subfields are disallowed.",
                        selection);

                if (selectionType is UnionType && hasSubSelection == null)
                    context.Error(
                        ValidationErrorCodes.R533LeafFieldSelections,
                        "Leaf selections on objects, interfaces, and unions " +
                        "without subfields are disallowed.",
                        selection);
            }
        }
    }
}