using System;
using System.Collections.Generic;
using fugu.graphql.error;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public class ExecutorContext : IExecutorContext
    {
        private readonly List<GraphQLError> _errors;

        public ExecutorContext(ISchema schema, GraphQLDocument document, IExecutionStrategy strategy)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _errors = new List<GraphQLError>();
        }

        public ISchema Schema { get; }

        public GraphQLDocument Document { get; }

        public IEnumerable<GraphQLError> FieldErrors => _errors;

        public IExecutionStrategy Strategy { get; }

        public void AddError(GraphQLError error)
        {
            if (_errors.Contains(error))
                return;

            _errors.Add(error);
        }
    }
}