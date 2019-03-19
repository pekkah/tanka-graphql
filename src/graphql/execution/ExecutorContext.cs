using System;
using System.Collections.Generic;
using tanka.graphql.error;
using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.execution
{
    public class ExecutorContext : IExecutorContext
    {
        private readonly List<Exception> _errors;

        public ExecutorContext(ISchema schema, GraphQLDocument document, Extensions extensions,
            IExecutionStrategy strategy)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _errors = new List<Exception>();
        }

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