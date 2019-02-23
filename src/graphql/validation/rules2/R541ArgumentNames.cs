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
        public override void BeginVisitArgument(
            GraphQLArgument argument,
            IValidationContext context)
        {
            base.BeginVisitArgument(argument, context);

            if (GetArgument() == null)
                context.Error(
                    ValidationErrorCodes.R541ArgumentNames,
                    "Every argument provided to a field or directive " +
                    "must be defined in the set of possible arguments of that " +
                    "field or directive.",
                    argument);
        }
    }
}