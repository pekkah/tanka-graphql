using System.Collections.Generic;
using tanka.graphql.error;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public interface IExecutorContext
    {
        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        IExecutionStrategy Strategy { get; }

        IEnumerable<GraphQLError> FieldErrors { get; }

        Extensions Extensions { get; }

        void AddError(GraphQLError error);
    }
}