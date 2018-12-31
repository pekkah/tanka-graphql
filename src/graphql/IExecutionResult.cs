using System.Collections.Generic;

namespace tanka.graphql
{
    public interface IExecutionResult
    {
        IEnumerable<Error> Errors { get; set; }
    }
}