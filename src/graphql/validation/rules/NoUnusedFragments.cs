using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     No unused fragments
    ///     A GraphQL document is only valid if all fragment definitions are spread
    ///     within operations, or spread within other fragments spread within operations.
    /// </summary>
    public class NoUnusedFragments : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            var operationDefs = new List<GraphQLOperationDefinition>();
            var fragmentDefs = new List<GraphQLFragmentDefinition>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLOperationDefinition>(node => operationDefs.Add(node));
                _.Match<GraphQLFragmentDefinition>(node => fragmentDefs.Add(node));
                _.Match<GraphQLDocument>(
                    leave: document =>
                    {
                        var fragmentNameUsed = new List<string>();
                        operationDefs.ForEach(operation =>
                        {
                            context.GetRecursivelyReferencedFragments(operation).ToList().ForEach(fragment =>
                            {
                                fragmentNameUsed.Add(fragment.Name.Value);
                            });
                        });

                        fragmentDefs.ForEach(fragmentDef =>
                        {
                            var fragName = fragmentDef.Name.Value;
                            if (!fragmentNameUsed.Contains(fragName))
                            {
                                var error = new ValidationError(UnusedFragMessage(fragName), fragmentDef);
                                context.ReportError(error);
                            }
                        });
                    });
            });
        }

        public string UnusedFragMessage(string fragName)
        {
            return $"Fragment \"{fragName}\" is never used.";
        }
    }
}