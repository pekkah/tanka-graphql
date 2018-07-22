using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace fugu.graphql.server.subscriptions
{
    public class SchemaExecutor : IExecutor
    {
        private readonly ExecutableSchema _schema;

        public SchemaExecutor(ExecutableSchema schema)
        {
            _schema = schema;
        }

        public async Task<IExecutionResult> ExecuteAsync(
            string query, 
            string operationName,
            Dictionary<string, object> variables)
        {
            var document = Parser.ParseDocument(query);

            // is subscription
            if (document.Definitions.OfType<GraphQLOperationDefinition>()
                .Any(op => op.Operation == OperationType.Subscription))
                return await SubscribeAsync(
                    document,
                    operationName,
                    variables);

   
            //is query or mutation
            return await ExecuteAsync(
                document,
                operationName,
                variables);
        }

        private Task<SubscriptionResult> SubscribeAsync(GraphQLDocument document, string operationName,
            Dictionary<string, object> variables)
        {
            return Executor.SubscribeAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = document,
                OperationName = operationName,
                VariableValues = variables,
                InitialValue = null
            });
        }

        private Task<ExecutionResult> ExecuteAsync(GraphQLDocument document, string operationName,
            Dictionary<string, object> variables)
        {
            return Executor.ExecuteAsync(new ExecutionOptions
            {
                Schema = _schema,
                Document = document,
                OperationName = operationName,
                VariableValues = variables,
                InitialValue = null
            });
        }
    }
}