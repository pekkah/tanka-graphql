using System;
using tanka.graphql.type;
using tanka.graphql.type.converters;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Variable default values of correct type
    ///     A GraphQL document is only valid if all variable default values are of the
    ///     type expected by their definition.
    /// </summary>
    public class DefaultValuesOfCorrectType : IValidationRule
    {
        public Func<string, string, string, string, string> BadValueForDefaultArgMessage =
            (message, varName, type, value) =>
                $"Variable \"{varName}\" of type \"{type}\" has invalid default value {value}. {message}";

        public Func<string, string, string, string> BadValueForNonNullArgMessage =
            (varName, type, guessType) => $"Variable \"{varName}\" of type \"{type}\" is required and" +
                                          " will not use default value. " +
                                          $"Perhaps you mean to use type \"{guessType}\"?";

        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLVariableDefinition>(node =>
                {
                    var name = node.Variable.Name.Value;
                    var defaultValue = node.DefaultValue;
                    var inputType = context.TypeInfo.GetInputType();

                    if (inputType is NonNull nonNull && defaultValue != null)
                        context.ReportError(new ValidationError(
                            BadValueForNonNullArgMessage(
                                name,
                                nonNull.Name,
                                nonNull.WrappedType.Name),
                            node));

                    if (inputType != null && defaultValue != null)
                        ValidateValue(context, node, defaultValue, inputType);
                });
            });
        }

        private void ValidateValue(ValidationContext context, GraphQLVariableDefinition node, object nodeValue,
            IGraphQLType type)
        {
            if (type is NonNull nonNull) ValidateValue(context, node, nodeValue, nonNull.WrappedType);

            if (type is List list)
            {
                if (nodeValue is GraphQLListValue listValue)
                    foreach (var listValueValue in listValue.Values)
                        ValidateValue(context, node, listValueValue, list.WrappedType);
                else
                    context.ReportError(new ValidationError(
                        BadValueForDefaultArgMessage(
                            "Expected type is list but value is not list value",
                            node.Variable.Name.Value,
                            type.Name,
                            null)));
            }

            if (type is IValueConverter leafType)
            {
                if (nodeValue is GraphQLScalarValue scalarValue)
                {
                    var value = leafType.ParseLiteral(scalarValue);
                    if (value == null)
                        context.ReportError(new ValidationError(
                            BadValueForNonNullArgMessage(
                                "Expected non-null value but null was parsed",
                                node.Variable.Name.Value,
                                type.Name), node));
                }
                else if (nodeValue is GraphQLVariable variableValue)
                {
                    //variables are expected to be ok
                }
                else
                {
                    context.ReportError(new ValidationError(
                        BadValueForDefaultArgMessage(
                            $"Expected leaf type value but was {nodeValue.GetType()}",
                            node.Variable.Name.Value,
                            type.Name,
                            nodeValue?.ToString()), node));
                }
            }
        }
    }
}