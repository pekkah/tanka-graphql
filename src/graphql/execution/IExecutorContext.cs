using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public interface IExecutorContext
    {
        GraphQLOperationDefinition Operation { get; }

        ISchema Schema { get; }

        GraphQLDocument Document { get; }

        ExtensionsRunner ExtensionsRunner { get; }

        IEnumerable<Exception> FieldErrors { get; }

        IExecutionStrategy Strategy { get; }

        IReadOnlyDictionary<string, object> CoercedVariableValues { get; }

        void AddError(Exception error);
    }
}