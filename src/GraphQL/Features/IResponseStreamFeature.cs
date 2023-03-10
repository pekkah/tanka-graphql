namespace Tanka.GraphQL.Features;

public interface IResponseStreamFeature
{
    IAsyncEnumerable<ExecutionResult> Response { get; set; }
}