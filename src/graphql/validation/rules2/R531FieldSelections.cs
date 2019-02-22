using System.Collections.Generic;
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
        public override IEnumerable<ValidationError> BeginVisitFieldSelection(
            GraphQLFieldSelection selection,
            IValidationContext context)
        {
            foreach (var validationError in base.BeginVisitFieldSelection(selection, context))
            {
                yield return validationError;
            }

            var fieldName = selection.Name.Value;

            if (fieldName == "__typename")
                yield break;


            if (getFieldDef() == null)
                yield return new ValidationError(
                    Errors.R531FieldSelections,
                    "The target field of a field selection must be defined " +
                    "on the scoped type of the selection set. There are no " +
                    "limitations on alias names.",
                    selection);

        }
    }
}