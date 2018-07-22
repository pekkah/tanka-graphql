using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;

namespace fugu.graphql.validation.rules
{
    /// <summary>
    ///     No fragment cycles
    /// </summary>
    public class NoFragmentCycles : IValidationRule
    {
        public INodeVisitor CreateVisitor(ValidationContext context)
        {
            // Tracks already visited fragments to maintain O(N) and to ensure that cycles
            // are not redundantly reported.
            var visitedFrags = new Dictionary<string, bool>();

            // Array of AST nodes used to produce meaningful errors
            var spreadPath = new Stack<GraphQLFragmentSpread>();

            // Position in the spread path
            var spreadPathIndexByName = new Dictionary<string, int>();

            return new EnterLeaveListener(_ =>
            {
                _.Match<GraphQLFragmentDefinition>(node =>
                {
                    if (!visitedFrags.ContainsKey(node.Name.Value))
                        DetectCycleRecursive(node, spreadPath, visitedFrags, spreadPathIndexByName, context);
                });
            });
        }

        public string CycleErrorMessage(string fragName, string[] spreadNames)
        {
            var via = spreadNames.Any() ? " via " + string.Join(", ", spreadNames) : "";
            return $"Cannot spread fragment \"{fragName}\" within itself{via}.";
        }

        private void DetectCycleRecursive(
            GraphQLFragmentDefinition fragment,
            Stack<GraphQLFragmentSpread> spreadPath,
            Dictionary<string, bool> visitedFrags,
            Dictionary<string, int> spreadPathIndexByName,
            ValidationContext context)
        {
            var fragmentName = fragment.Name.Value;
            visitedFrags[fragmentName] = true;

            var spreadNodes = context.GetFragmentSpreads(fragment.SelectionSet).ToArray();
            if (!spreadNodes.Any()) return;

            spreadPathIndexByName[fragmentName] = spreadPath.Count;

            foreach (var spreadNode in spreadNodes)
            {
                var spreadName = spreadNode.Name.Value;
                var cycleIndex = spreadPathIndexByName[spreadName];

                if (cycleIndex == -1)
                {
                    spreadPath.Push(spreadNode);

                    if (!visitedFrags[spreadName])
                    {
                        var spreadFragment = context.GetFragment(spreadName);
                        if (spreadFragment != null)
                            DetectCycleRecursive(
                                spreadFragment,
                                spreadPath,
                                visitedFrags,
                                spreadPathIndexByName,
                                context);
                    }

                    spreadPath.Pop();
                }
                else
                {
                    var cyclePath = spreadPath.Reverse().Skip(cycleIndex).ToArray();
                    var nodes = cyclePath.OfType<ASTNode>().Concat(new[] {spreadNode}).ToArray();

                    context.ReportError(new ValidationError(
                        CycleErrorMessage(spreadName, cyclePath.Select(x => x.Name.Value).ToArray()),
                        nodes));
                }
            }

            spreadPathIndexByName[fragmentName] = -1;
        }
    }
}