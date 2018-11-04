using System.Collections.Generic;
using System.Threading.Tasks;

namespace fugu.graphql.server.subscriptions
{
    public interface IExecutor
    {
        Task<IExecutionResult> ExecuteAsync(string query, string operationName, Dictionary<string, object> variables);
    }
}