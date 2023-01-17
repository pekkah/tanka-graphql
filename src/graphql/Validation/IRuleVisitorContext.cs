using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public interface IRuleVisitorContext
{
    ExecutableDocument Document { get; }

    ExtensionData Extensions { get; }
    ISchema Schema { get; }

    TypeTracker Tracker { get; }

    IReadOnlyDictionary<string, object?> VariableValues { get; }

    void Error(string code, string message, params INode[] nodes);

    void Error(string code, string message, INode node);

    void Error(string code, string message, IEnumerable<INode> nodes);

    List<VariableUsage> GetVariables(
        INode rootNode);

    IEnumerable<VariableUsage> GetRecursiveVariables(
        OperationDefinition operation);

    FragmentDefinition? GetFragment(string name);
    List<FragmentSpread> GetFragmentSpreads(SelectionSet node);

    IEnumerable<FragmentDefinition> GetRecursivelyReferencedFragments(
        OperationDefinition operation);
}