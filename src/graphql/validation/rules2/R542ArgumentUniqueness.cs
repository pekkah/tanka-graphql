using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    /// For each argument in the Document.
    /// Let argumentName be the Name of argument.
    /// Let arguments be all Arguments named argumentName in the Argument Set which contains argument.
    /// arguments must be the set containing only argument.
    /// </summary>
    public class R542ArgumentUniqueness : TypeTrackingRuleBase
    {
        public override void BeginVisitArguments(IEnumerable<GraphQLArgument> arguments, IValidationContext context)
        {
            var args = arguments.ToList();
            base.BeginVisitArguments(args, context);

            var knownArgs = new List<string>();
            foreach (var argument in args)
            {
                if (knownArgs.Contains(argument.Name.Value))
                {
                    context.Error(
                        ValidationErrorCodes.R542ArgumentUniqueness,
                        "Fields and directives treat arguments as a mapping of " +
                        "argument name to value. More than one argument with the same " +
                        "name in an argument set is ambiguous and invalid.",
                        argument);
                }

                knownArgs.Add(argument.Name.Value);
            }
        }
    }
}