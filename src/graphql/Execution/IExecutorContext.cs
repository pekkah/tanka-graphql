using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution
{
    public interface IExecutorContext
    {
        OperationDefinition Operation { get; }

        ISchema Schema { get; }

        ExecutableDocument Document { get; }

        ExtensionsRunner ExtensionsRunner { get; }

        IEnumerable<Exception> FieldErrors { get; }

        IExecutionStrategy Strategy { get; }

        IReadOnlyDictionary<string, object> CoercedVariableValues { get; }

        void AddError(Exception error);
    }
}