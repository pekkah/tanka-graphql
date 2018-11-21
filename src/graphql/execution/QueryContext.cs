using System;
using System.Collections.Generic;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public class QueryContext
    {
        public QueryContext(Func<GraphQLError, Error> formatError,
            GraphQLDocument document,
            GraphQLOperationDefinition operation,
            ISchema schema,
            Dictionary<string, object> coercedVariableValues,
            object initialValue, 
            Extensions extensions)
        {
            FormatError = formatError ?? throw new ArgumentNullException(nameof(formatError));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            OperationDefinition = operation ?? throw new ArgumentNullException(nameof(operation));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            CoercedVariableValues = coercedVariableValues ?? throw new ArgumentNullException(nameof(coercedVariableValues));
            InitialValue = initialValue;
            Extensions = extensions;
        }

        public Func<GraphQLError, Error> FormatError { get; }

        public GraphQLDocument Document { get; }

        public GraphQLOperationDefinition OperationDefinition { get; }

        public ISchema Schema { get; }

        public Dictionary<string, object> CoercedVariableValues { get; }

        public object InitialValue { get; }

        public Extensions Extensions { get; }

        public void Deconstruct(out ISchema schema, out GraphQLDocument document, out GraphQLOperationDefinition operation, out object initialValue,
            out Dictionary<string, object> coercedVariableValues)
        {
            schema = Schema;
            document = Document;
            operation = OperationDefinition;
            initialValue = InitialValue;
            coercedVariableValues = CoercedVariableValues;
        }

        public IExecutorContext BuildExecutorContext(IExecutionStrategy executionStrategy)
        {
            return new ExecutorContext(
                Schema,
                Document,
                Extensions,
                executionStrategy);
        }
    }
}