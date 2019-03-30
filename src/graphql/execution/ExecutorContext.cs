using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.execution
{
    public class ExecutorContext : IExecutorContext
    {
        private readonly List<Exception> _errors;

        public ExecutorContext(ISchema schema,
            GraphQLDocument document,
            Extensions extensions,
            IExecutionStrategy strategy,
            GraphQLOperationDefinition operation,
            IDictionary<string, GraphQLFragmentDefinition> fragments, 
            IDictionary<string, object> coercedVariableValues)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            Operation = operation;
            Fragments = fragments;
            CoercedVariableValues = coercedVariableValues;
            _errors = new List<Exception>();
        }

        public GraphQLOperationDefinition Operation { get; }
        public IDictionary<string, GraphQLFragmentDefinition> Fragments { get; }
        public IDictionary<string, object> CoercedVariableValues { get; }

        public ISchema Schema { get; }

        public GraphQLDocument Document { get; }

        public Extensions Extensions { get; }

        public IEnumerable<Exception> FieldErrors => _errors;

        public IExecutionStrategy Strategy { get; }

        public void AddError(Exception error)
        {
            if (_errors.Contains(error))
                return;

            _errors.Add(error);
        }
    }
}