using System.Collections.Generic;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Unique variable names
    ///     A GraphQL operation is onlys valid if all its variables are uniquely named.
    /// </summary>
    public class UniqueVariableNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var knownVariables = new Dictionary<string, GraphQLVariableDefinition>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLOperationDefinition>(op =>
                    knownVariables = new Dictionary<string, GraphQLVariableDefinition>());

                _.Match<GraphQLVariableDefinition>(variableDefinition =>
                {
                    var variableName = variableDefinition.Variable.Name.Value;
                    if (knownVariables.ContainsKey(variableName))
                    {
                        var error = new ValidationError(
                            DuplicateVariableMessage(variableName),
                            knownVariables[variableName],
                            variableDefinition);
                        context.ReportError(error);
                    }
                    else
                    {
                        knownVariables[variableName] = variableDefinition;
                    }
                });
            });
        }

        public string DuplicateVariableMessage(string variableName)
        {
            return $"There can be only one variable named \"{variableName}\"";
        }
    }
}