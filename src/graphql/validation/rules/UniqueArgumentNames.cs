using System.Collections.Generic;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    public class UniqueArgumentNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var knownArgs = new Dictionary<string, GraphQLArgument>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFieldSelection>(field => knownArgs = new Dictionary<string, GraphQLArgument>());
                _.Match<GraphQLDirective>(field => knownArgs = new Dictionary<string, GraphQLArgument>());

                _.Match<GraphQLArgument>(argument =>
                {
                    var argName = argument.Name.Value;
                    if (knownArgs.ContainsKey(argName))
                    {
                        var error = new ValidationError(
                            DuplicateArgMessage(argName),
                            knownArgs[argName],
                            argument);
                        context.ReportError(error);
                    }
                    else
                    {
                        knownArgs[argName] = argument;
                    }
                });
            });
        }

        public string DuplicateArgMessage(string argName)
        {
            return $"There can be only one argument named \"{argName}\".";
        }
    }
}