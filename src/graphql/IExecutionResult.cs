using System.Collections.Generic;

namespace Tanka.GraphQL
{
    public interface IExecutionResult
    {
        List<ExecutionError> Errors { get; set; }

        Dictionary<string, object> Extensions { get; set; }
    }
}