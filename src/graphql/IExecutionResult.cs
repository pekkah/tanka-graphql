using System.Collections.Generic;

namespace Tanka.GraphQL
{
    public interface IExecutionResult
    {
        IEnumerable<ExecutionError> Errors { get; set; }
        IDictionary<string, object> Extensions { get; set; }
    }
}