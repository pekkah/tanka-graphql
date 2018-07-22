using System.Collections.Generic;

namespace fugu.graphql
{
    public interface IExecutionResult
    {
        IEnumerable<Error> Errors { get; set; }
    }
}