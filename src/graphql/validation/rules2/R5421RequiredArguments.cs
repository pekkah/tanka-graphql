using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.execution;
using tanka.graphql.type;

namespace tanka.graphql.validation.rules2
{
    /// <summary>
    ///     For each Field or Directive in the document.
    ///     Let arguments be the arguments provided by the Field or Directive.
    ///     Let argumentDefinitions be the set of argument definitions of that Field or Directive.
    ///     For each argumentDefinition in argumentDefinitions:
    ///         - Let type be the expected type of argumentDefinition.
    ///         - Let defaultValue be the default value of argumentDefinition.
    ///         - If type is Non‐Null and defaultValue does not exist:
    ///             - Let argumentName be the name of argumentDefinition.
    ///             - Let argument be the argument in arguments named argumentName
    ///             argument must exist.
    ///             - Let value be the value of argument.
    ///             value must not be the null literal.
    /// </summary>
    public class R5421RequiredArguments : TypeTrackingRuleBase
    {
        public override void BeginVisitArguments(IEnumerable<GraphQLArgument> arguments, IValidationContext context)
        {
            var args = arguments.ToList();
            base.BeginVisitArguments(args, context);

            
            var argumentDefinitions = GetArgumentDefinitions();

            //todo: should this produce error?
            if (argumentDefinitions == null)
                return;

            foreach (var argumentDefinition in argumentDefinitions)
            {
                var type = argumentDefinition.Value.Type;
                var defaultValue = argumentDefinition.Value.DefaultValue;

                if (type is NonNull nonNull && defaultValue == null)
                {
                    var argumentName = argumentDefinition.Key;
                    var argument = args.SingleOrDefault(a => a.Name.Value == argumentName);

                    if (argument == null)
                    {
                        context.Error(
                            ValidationErrorCodes.R5421RequiredArguments,
                            "Arguments is required. An argument is required " +
                            "if the argument type is non‐null and does not have a default " +
                            "value. Otherwise, the argument is optional. " +
                            $"Argument {argumentName} not given",
                            args);

                        return;
                    }

                    // We don't want to throw error here due to non-null so we use the WrappedType directly
                    var argumentValue = Values.CoerceValue(context.Schema, argument.Value, nonNull.WrappedType);
                    if (argumentValue == null)
                    {
                        context.Error(
                            ValidationErrorCodes.R5421RequiredArguments,
                            "Arguments is required. An argument is required " +
                            "if the argument type is non‐null and does not have a default " +
                            "value. Otherwise, the argument is optional. " +
                            $"Value of argument {argumentName} cannot be null",
                            args);
                    }
                }
            }
        }

        private IEnumerable<KeyValuePair<string, Argument>> GetArgumentDefinitions()
        {
            var definitions = GetDirective()?.Arguments
                              ?? GetFieldDef()?.Field.Arguments;

            return definitions;
        }
    }
}