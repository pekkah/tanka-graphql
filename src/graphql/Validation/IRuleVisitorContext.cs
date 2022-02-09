using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public interface IRuleVisitorContext 
    {
        ISchema Schema { get; }

        ExecutableDocument Document { get; }

        IReadOnlyDictionary<string, object?> VariableValues { get; }

        TypeTracker Tracker { get; }

        ExtensionData Extensions { get; }

        void Error(string code, string message, params INode[] nodes);

        void Error(string code, string message, INode node);

        void Error(string code, string message, IEnumerable<INode> nodes);

        List<VariableUsage> GetVariables(
            INode rootNode);

        IEnumerable<VariableUsage> GetRecursiveVariables(
            OperationDefinition operation);

        FragmentDefinition GetFragment(string name);
        List<FragmentSpread> GetFragmentSpreads(SelectionSet node);

        IEnumerable<FragmentDefinition> GetRecursivelyReferencedFragments(
            OperationDefinition operation);
    }
}