namespace Tanka.GraphQL;

public partial class Executor
{
    /// <summary>
    ///     Execute query, mutation or subscription operation using the given <paramref name="context"/>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ExecutionResult> ExecuteOperation(QueryContext context)
    {
        await _operationDelegate(context);

        await foreach (ExecutionResult er in context.Response.WithCancellation(context.RequestCancelled))
            yield return er;
    }
}