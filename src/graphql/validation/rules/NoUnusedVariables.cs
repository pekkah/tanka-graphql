using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     No unused variables
    ///     A GraphQL operation is only valid if all variables defined by that operation
    ///     are used in that operation or a fragment transitively included by that
    ///     operation.
    /// </summary>
    public class NoUnusedVariables : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var variableDefs = new List<GraphQLVariableDefinition>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariableDefinition>(def => variableDefs.Add(def));

                _.Match<GraphQLOperationDefinition>(
                    op => variableDefs = new List<GraphQLVariableDefinition>(),
                    op =>
                    {
                        var usages = context.GetRecursiveVariables(op).Select(usage => usage.Node.Name.Value);
                        variableDefs.ForEach(variableDef =>
                        {
                            var variableName = variableDef.Variable.Name.Value;
                            if (!usages.Contains(variableName))
                            {
                                var error = new ValidationError(UnusedVariableMessage(variableName, op.Name.Value),
                                    variableDef);
                                context.ReportError(error);
                            }
                        });
                    });
            });
        }

        public string UnusedVariableMessage(string varName, string opName)
        {
            return !string.IsNullOrWhiteSpace(opName)
                ? $"Variable \"${varName}\" is never used in operation \"${opName}\"."
                : $"Variable \"${varName}\" is never used.";
        }
    }
}