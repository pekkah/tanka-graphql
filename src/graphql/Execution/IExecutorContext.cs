using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution;

public interface IExecutorContext
{
    IReadOnlyDictionary<string, object?> CoercedVariableValues { get; }

    ExecutableDocument Document { get; }

    ExtensionsRunner ExtensionsRunner { get; }

    IEnumerable<Exception> FieldErrors { get; }
    OperationDefinition Operation { get; }

    ISchema Schema { get; }

    IExecutionStrategy Strategy { get; }

    void AddError(Exception error);
}