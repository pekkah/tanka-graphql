using tanka.graphql.type;
using tanka.graphql.type.converters;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Argument values of correct type
    ///     A GraphQL document is only valid if all field argument literal values are
    ///     of the type expected by their position.
    /// </summary>
    public class ArgumentsOfCorrectType : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLArgument>(node =>
                {
                    var argDef = context.TypeInfo.GetArgument();
                    if (argDef == null)
                        return;

                    var type = argDef.Type;

                    // variables should be of the expected type
                    if (node.Value is GraphQLVariable) return;

                    ValidateValue(context, node, node.Value, type);
                });
            });
        }

        private void ValidateValue(ValidationContext context, GraphQLArgument node, GraphQLValue nodeValue, IGraphQLType type)
        {
            if (type is NonNull nonNull) ValidateValue(context, node, nodeValue, nonNull.WrappedType);

            if (type is List list)
            {
                if (nodeValue is GraphQLListValue listValue)
                    foreach (var listValueValue in listValue.Values)
                        ValidateValue(context, node, listValueValue, list.WrappedType);
                else
                    context.ReportError(new ValidationError(
                        BadValueMessage(
                            "Expected type is list but value is not list value",
                            node.Name.Value,
                            type,
                            null)));
            }

            if (type is IValueConverter leafType)
            {
                if (nodeValue is GraphQLScalarValue scalarValue)
                {
                    var value = leafType.ParseLiteral(scalarValue);
                    if (value == null)
                        context.ReportError(new ValidationError(
                            BadValueMessage(
                                "Expected non-null value but null was parsed",
                                node.Name.Value,
                                type,
                                null), node));
                }
                else if (nodeValue is GraphQLVariable variableValue)
                {
                    //variables are expected to be ok
                }
                else
                {
                    context.ReportError(new ValidationError(
                        BadValueMessage(
                            $"Expected leaf type value but was {nodeValue.Kind}",
                            node.Name.Value,
                            type,
                            null), node));
                }
            }
        }

        public string BadValueMessage(
            string mesage,
            string argName,
            IGraphQLType type,
            string value)
        {
            return $"Argument \"{argName}\" has invalid value {value}. {mesage}.";
        }
    }
}