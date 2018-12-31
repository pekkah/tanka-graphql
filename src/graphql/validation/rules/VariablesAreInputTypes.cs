using System;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    /// Variables are input types
    ///
    /// A GraphQL operation is only valid if all the variables it defines are of
    /// input types (scalar, enum, or input object).
    /// </summary>
    public class VariablesAreInputTypes : IValidationRule
    {
        public Func<string, string, string> UndefinedVarMessage = (variableName, typeName) =>
            $"Variable \"{variableName}\" cannot be non-input type \"{typeName}\".";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariableDefinition>(varDef =>
                {
                    var type = Ast.TypeFromAst(context.Schema, varDef.Type)?.Unwrap();

                    if (type is InputObjectType)
                        return;

                    if (type is IValueConverter)
                        return;

                   context.ReportError(new ValidationError(UndefinedVarMessage(varDef.Variable.Name.Value, type?.Name), varDef));
                });
            });
        }
    }
}
