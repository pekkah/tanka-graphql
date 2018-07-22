using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     Fragments on composite type
    ///     Fragments use a type condition to determine if they apply, since fragments
    ///     can only be spread into a composite type (object, interface, or union), the
    ///     type condition must also be a composite type.
    /// </summary>
    public class FragmentsOnCompositeTypes : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLInlineFragment>(node =>
                {
                    var type = context.TypeInfo.GetLastType();
                    if (node.TypeCondition != null && type != null && !(type is ComplexType))
                        context.ReportError(new ValidationError(
                            GraphQLInlineFragmentOnNonCompositeErrorMessage(type.ToString()),
                            node));
                });

                _.Match<GraphQLFragmentDefinition>(node =>
                {
                    var type = context.TypeInfo.GetLastType();
                    if (type != null && !(type is ComplexType))
                        context.ReportError(new ValidationError(
                            FragmentOnNonCompositeErrorMessage(node.Name.Value, type.ToString()),
                            node));
                });
            });
        }

        public string GraphQLInlineFragmentOnNonCompositeErrorMessage(string type)
        {
            return $"Fragment cannot condition on non composite type \"{type}\".";
        }

        public string FragmentOnNonCompositeErrorMessage(string fragName, string type)
        {
            return $"Fragment \"{fragName}\" cannot condition on non composite type \"{type}\".";
        }
    }
}