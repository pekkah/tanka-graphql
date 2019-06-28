using System.Collections.Generic;

namespace tanka.graphql
{
    public interface IExecutionResult
    {
        IEnumerable<ExecutionError> Errors { get; set; }
        IDictionary<string, object> Extensions { get; set; }
    }
}