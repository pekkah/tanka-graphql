namespace Tanka.GraphQL;

public partial class Executor
{
    public static async IAsyncEnumerable<ExecutionResult> ExecuteOperation(QueryContext context)
    {
        await context.ExecuteOperation();

        await foreach (ExecutionResult er in context.Response.WithCancellation(context.RequestCancelled))
            yield return er;
    }
}