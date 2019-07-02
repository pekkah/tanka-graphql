using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public class QueryContext
    {
        public QueryContext(
            Func<Exception, ExecutionError> formatError,
            GraphQLDocument document,
            GraphQLOperationDefinition operation,
            ISchema schema,
            IReadOnlyDictionary<string, object> coercedVariableValues,
            object initialValue,
            ExtensionsRunner extensionsRunner)
        {
            FormatError = formatError ?? throw new ArgumentNullException(nameof(formatError));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            OperationDefinition = operation ?? throw new ArgumentNullException(nameof(operation));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            CoercedVariableValues =
                coercedVariableValues ?? throw new ArgumentNullException(nameof(coercedVariableValues));
            InitialValue = initialValue;
            ExtensionsRunner = extensionsRunner;
        }

        public Func<Exception, ExecutionError> FormatError { get; }

        public GraphQLDocument Document { get; }

        public GraphQLOperationDefinition OperationDefinition { get; }

        public ISchema Schema { get; }

        public IReadOnlyDictionary<string, object> CoercedVariableValues { get; }

        public object InitialValue { get; }

        public ExtensionsRunner ExtensionsRunner { get; }

        public void Deconstruct(out ISchema schema, out GraphQLDocument document,
            out GraphQLOperationDefinition operation, out object initialValue,
            out IReadOnlyDictionary<string, object> coercedVariableValues)
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
                Document.Definitions.OfType<GraphQLFragmentDefinition>()
                    .ToDictionary(f => f.Name.Value, f => f),
                CoercedVariableValues);
        }
    }
}