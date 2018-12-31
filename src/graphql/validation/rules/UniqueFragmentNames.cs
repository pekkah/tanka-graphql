using System.Collections.Generic;
using GraphQLParser.AST;

namespace tanka.graphql.validation.rules
{
    /// <summary>
    ///     Unique fragment names
    ///     A GraphQL document is only valid if all defined fragments have unique names.
    /// </summary>
    public class UniqueFragmentNames : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var knownFragments = new Dictionary<string, GraphQLFragmentDefinition>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFragmentDefinition>(fragmentDefinition =>
                {
                    var fragmentName = fragmentDefinition.Name.Value;
                    if (knownFragments.ContainsKey(fragmentName))
                    {
                        var error = new ValidationError(
                            DuplicateFragmentNameMessage(fragmentName),
                            knownFragments[fragmentName],
                            fragmentDefinition);
                        context.ReportError(error);
                    }
                    else
                    {
                        knownFragments[fragmentName] = fragmentDefinition;
                    }
                });
            });
        }

        public string DuplicateFragmentNameMessage(string fragName)
        {
            return $"There can only be one fragment named \"{fragName}\"";
        }
    }
}