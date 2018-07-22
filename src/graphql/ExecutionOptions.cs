using System.Collections.Generic;
using fugu.graphql.error;
using GraphQLParser.AST;

namespace fugu.graphql
{
    public class ExecutionOptions
    {
        public ExecutableSchema Schema { get; set; }

        public GraphQLDocument Document { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> VariableValues { get; set; }

        public object InitialValue { get; set; }

        public IErrorTransformer ErrorTransformer { get; set; } = new DefaultErrorTransformer();

        public bool Validate { get; } = true;
    }
}