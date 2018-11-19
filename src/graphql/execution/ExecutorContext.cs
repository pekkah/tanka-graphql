using System;
using System.Collections.Generic;
using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.execution
{
    public class ExecutorContext : IExecutorContext
    {
        public ExecutorContext(ISchema schema, GraphQLDocument document, IExecutionStrategy strategy)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public ISchema Schema { get; }

        public GraphQLDocument Document { get; }

        public List<Exception> FieldErrors { get; } = new List<Exception>();

        public IExecutionStrategy Strategy { get; }
    }
}