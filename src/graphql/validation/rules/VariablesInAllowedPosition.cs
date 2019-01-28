using System;
using System.Collections.Generic;
using System.Linq;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Variables passed to field arguments conform to type
    /// </summary>
    public class VariablesInAllowedPosition : IValidationRule
    {
        public Func<string, string, string, string> BadVarPosMessage =>
            (varName, varType, expectedType) =>
                $"Variable \"${varName}\" of type \"{varType}\" used in position " +
                $"expecting type \"{expectedType}\".";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var varDefMap = new Dictionary<string, GraphQLVariableDefinition>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariableDefinition>(
                    varDefAst => varDefMap[varDefAst.Variable.Name.Value] = varDefAst
                );

                _.Match<GraphQLOperationDefinition>(
                    op => varDefMap = new Dictionary<string, GraphQLVariableDefinition>(),
                    op =>
                    {
                        var usages = context.GetRecursiveVariables(op).ToList();
                        usages.ForEach(usage =>
                        {
                            var varName = usage.Node.Name.Value;
                            if (!varDefMap.TryGetValue(varName, out var varDef)) return;

                            if (varDef != null && usage.Type != null)
                            {
                                var varType = Ast.TypeFromAst(context.Schema, varDef.Type);
                                /*
                                if (varType != null &&
                                    !EffectiveType(varType, varDef).IsSubtypeOf(usage.Type, context.Schema))
                                {
                                    var error = new ValidationError(
                                        BadVarPosMessage(varName, context.Print(varType), context.Print(usage.Type)));

                                    var source = new Source(context.OriginalQuery);
                                    var varDefPos = new Location(source, varDef.SourceLocation.Start);
                                    var usagePos = new Location(source, usage.Node.SourceLocation.Start);

                                    error.AddLocation(varDefPos.Line, varDefPos.Column);
                                    error.AddLocation(usagePos.Line, usagePos.Column);

                                    context.ReportError(error);
                                }*/
                            }
                        });
                    }
                );
            });
        }

        /// <summary>
        ///     if a variable definition has a default value, it is effectively non-null.
        /// </summary>
        private IType EffectiveType(IType varType, GraphQLVariableDefinition varDef)
        {
            if (varDef.DefaultValue == null || varType is NonNull) return varType;

            return new NonNull(varType);
        }
    }
}