using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public interface IExecutorContext
    {
        GraphQLOperationDefinition Operation { get; }

        IDictionary<string, GraphQLFragmentDefinition> Fragments { get; }

        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        Extensions Extensions { get; }

        IEnumerable<Exception> FieldErrors { get; }

        IExecutionStrategy Strategy { get; }

        void AddError(Exception error);
    }
}