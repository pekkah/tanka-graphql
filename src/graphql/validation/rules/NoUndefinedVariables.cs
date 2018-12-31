using System;
using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     No undefined variables
    ///     A GraphQL operation is only valid if all variables encountered, both directly
    ///     and via fragment spreads, are defined by that operation.
    /// </summary>
    public class NoUndefinedVariables : IValidationRule
    {
        public Func<string, string, string> UndefinedVarMessage = (varName, opName) =>
            !string.IsNullOrWhiteSpace(opName)
                ? $"Variable \"${varName}\" is not defined by operation \"{opName}\"."
                : $"Variable \"${varName}\" is not defined.";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var variableNameDefined = new Dictionary<string, bool>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariableDefinition>(varDef => variableNameDefined[varDef.Variable.Name.Value] = true);

                _.Match<GraphQLOperationDefinition>(
                    op => variableNameDefined = new Dictionary<string, bool>(),
                    op =>
                    {
                        var usages = context.GetRecursiveVariables(op);

                        foreach (var usage in usages)
                        {
                            var varName = usage.Node.Name.Value;
                            if (!variableNameDefined.TryGetValue(varName, out var _))
                            {
                                var error = new ValidationError(
                                    UndefinedVarMessage(varName, op.Name.Value),
                                    usage.Node,
                                    op);
                                context.ReportError(error);
                            }
                        }
                    });
            });
        }
    }
}