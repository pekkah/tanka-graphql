using System;
using System.Collections.Generic;
using System.Linq;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Execution;

public class QueryContext
{
    public QueryContext(
        Func<Exception, ExecutionError> formatError,
        ExecutableDocument document,
        OperationDefinition operation,
        ISchema schema,
        IReadOnlyDictionary<string, object?> coercedVariableValues,
        object initialValue,
        ExtensionsRunner extensionsRunner)
    {
        FormatError = formatError ?? throw new ArgumentNullException(nameof(formatError));
        Document = document ?? throw new ArgumentNullException(nameof(document));
        OperationDefinition = operation ?? throw new ArgumentNullException(nameof(operation));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        CoercedVariableValues = coercedVariableValues;
        InitialValue = initialValue;
        ExtensionsRunner = extensionsRunner;
    }

    public IReadOnlyDictionary<string, object?> CoercedVariableValues { get; }

    public ExecutableDocument Document { get; }

    public ExtensionsRunner ExtensionsRunner { get; }

    public Func<Exception, ExecutionError> FormatError { get; }

    public object InitialValue { get; }

    public OperationDefinition OperationDefinition { get; }

    public ISchema Schema { get; }

    public void Deconstruct(out ISchema schema, out ExecutableDocument document,
        out OperationDefinition operation, out object initialValue,
        out IReadOnlyDictionary<string, object?> coercedVariableValues)
    {
        schema = Schema;
        document = Document;
        operation = OperationDefinition;
        initialValue = InitialValue;
        coercedVariableValues = CoercedVariableValues;
    }

    public IExecutorContext BuildExecutorContext(
        IExecutionStrategy executionStrategy)
    {
        return new ExecutorContext(
            Schema,
            Document,
            ExtensionsRunner,
            executionStrategy,
            OperationDefinition,
            Document.FragmentDefinitions
                ?.ToDictionary(f => f.FragmentName.ToString(), f => f),
            CoercedVariableValues);
    }
}