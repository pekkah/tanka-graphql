using System.Collections.Generic;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
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