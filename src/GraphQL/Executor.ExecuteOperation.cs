namespace Tanka.GraphQL;

public partial class Executor
{
    public async IAsyncEnumerable<ExecutionResult> ExecuteOperation(QueryContext context)
    {
        await _operationDelegate(context);

        await foreach (ExecutionResult er in context.Response.WithCancellation(context.RequestCancelled))
            yield return er;
    }
}