namespace Tanka.GraphQL;

public interface IResponseStreamFeature
{
    IAsyncEnumerable<ExecutionResult> Response { get; set; }
}