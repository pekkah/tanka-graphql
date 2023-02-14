using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

/// <summary>
///     Execute queries, mutations and subscriptions
/// </summary>
public partial class Executor
{
    /// <summary>
    ///     Execute query or mutation
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ExecutionResult> Execute(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        QueryContext queryContext = BuildQueryContextAsync(request);
        queryContext.RequestCancelled = cancellationToken;

        IAsyncEnumerable<ExecutionResult> executionResult = ExecuteOperation(queryContext);

        return await executionResult.SingleAsync(queryContext.RequestCancelled);
    }

    public static Task<ExecutionResult> Execute(
        ISchema schema,
        ExecutableDocument document,
        Dictionary<string, object?>? variableValues = null,
        object? initialValue = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        var executor = new Executor(schema);
        return executor.Execute(new GraphQLRequest
        {
            Document = document,
            InitialValue = initialValue,
            OperationName = operationName,
            Variables = variableValues
        }, cancellationToken);
    }
}