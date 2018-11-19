using System;
using System.Collections.Generic;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public interface IExecutorContext
    {
        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        List<Exception> FieldErrors { get; }

        IExecutionStrategy Strategy { get; }
    }
}