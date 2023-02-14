namespace Tanka.GraphQL;

public class DefaultResponseStreamFeature : IResponseStreamFeature
{
    public IAsyncEnumerable<ExecutionResult> Response { get; set; } = AsyncEnumerable.Empty<ExecutionResult>();
}