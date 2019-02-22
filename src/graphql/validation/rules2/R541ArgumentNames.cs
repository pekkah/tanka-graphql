using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each argument in the document
    ///     Let argumentName be the Name of argument.
    ///     Let argumentDefinition be the argument definition provided by the parent field or definition named argumentName.
    ///     argumentDefinition must exist.
    /// </summary>
    public class R541ArgumentNames : TypeTrackingRuleBase
    {
        public override IEnumerable<ValidationError> BeginVisitArgument(
            GraphQLArgument argument,
            IValidationContext context)
        {
            foreach (var validationError in base.BeginVisitArgument(argument, context)) 
                yield return validationError;

            if (getArgument() == null)
                yield return new ValidationError(
                    Errors.R541ArgumentNames,
                    "Every argument provided to a field or directive " +
                    "must be defined in the set of possible arguments of that " +
                    "field or directive.",
                    argument);
        }
    }
}