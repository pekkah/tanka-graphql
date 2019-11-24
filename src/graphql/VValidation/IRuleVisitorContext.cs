using System.Collections.Generic;
using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public interface IRuleVisitorContext 
    {
        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        IReadOnlyDictionary<string, object> VariableValues { get; }

        TypeTracker Tracker { get; }

        ExtensionData Extensions { get; }

        void Error(string code, string message, params ASTNode[] nodes);

        void Error(string code, string message, ASTNode node);

        void Error(string code, string message, IEnumerable<ASTNode> nodes);

        List<VariableUsage> GetVariables(
            ASTNode rootNode);

        IEnumerable<VariableUsage> GetRecursiveVariables(
            GraphQLOperationDefinition operation);

        GraphQLFragmentDefinition GetFragment(string name);
        List<GraphQLFragmentSpread> GetFragmentSpreads(GraphQLSelectionSet node);

        IEnumerable<GraphQLFragmentDefinition> GetRecursivelyReferencedFragments(
            GraphQLOperationDefinition operation);
    }
}