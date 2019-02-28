using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.validation
{
    public interface IRuleVisitorContext 
    {
        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        IDictionary<string, object> VariableValues { get; }

        TypeTracker Tracker { get; }

        void Error(string code, string message, params ASTNode[] nodes);

        void Error(string code, string message, ASTNode node);

        void Error(string code, string message, IEnumerable<ASTNode> nodes);
    }
}