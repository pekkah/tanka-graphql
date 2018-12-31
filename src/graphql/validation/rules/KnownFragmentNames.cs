using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Known fragment names
    ///     A GraphQL document is only valid if all <c>...Fragment</c> fragment spreads refer
    ///     to fragments defined in the same document.
    /// </summary>
    public class KnownFragmentNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFragmentSpread>(node =>
                {
                    var fragmentName = node.Name.Value;
                    var fragment = context.GetFragment(fragmentName);
                    if (fragment == null)
                    {
                        var error = new ValidationError(UnknownFragmentMessage(fragmentName), node);
                        context.ReportError(error);
                    }
                });
            });
        }

        public string UnknownFragmentMessage(string fragName)
        {
            return $"Unknown fragment \"{fragName}\".";
        }
    }
}