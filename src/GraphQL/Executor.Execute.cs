using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;

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

    /// <summary>
    ///     Execute operation with given <paramref name="context"/>.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task ExecuteContext(QueryContext context)
    {
        await _operationDelegate(context);
    }

    /// <summary>
    ///     Static execute method for executing query or mutation operation.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="document"></param>
    /// <param name="variableValues"></param>
    /// <param name="initialValue"></param>
    /// <param name="operationName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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