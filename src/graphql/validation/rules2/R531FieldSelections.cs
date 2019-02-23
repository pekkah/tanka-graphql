using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each selection in the document.
    ///     Let fieldName be the target field of selection
    ///     fieldName must be defined on type in scope
    /// </summary>
    public class R531FieldSelections : TypeTrackingRuleBase
    {
        public override void BeginVisitFieldSelection(
            GraphQLFieldSelection selection,
            IValidationContext context)
        {
            base.BeginVisitFieldSelection(selection, context);

            var fieldName = selection.Name.Value;

            if (fieldName == "__typename")
                return;

            if (GetFieldDef() == null)
                context.Error(
                    ValidationErrorCodes.R531FieldSelections,
                    "The target field of a field selection must be defined " +
                    "on the scoped type of the selection set. There are no " +
                    "limitations on alias names.",
                    selection);
        }
    }
}