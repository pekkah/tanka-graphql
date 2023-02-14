using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

public partial class Executor
{
    /// <summary>
    ///     Execute query or mutation
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ExecutionResult> Subscribe(
        GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        QueryContext queryContext = BuildQueryContextAsync(request);
        queryContext.RequestCancelled = cancellationToken;

        return ExecuteOperation(queryContext);
    }


    public static IAsyncEnumerable<ExecutionResult> Subscribe(
        ISchema schema,
        ExecutableDocument document,
        CancellationToken cancellationToken,
        Dictionary<string, object?>? variableValues = null,
        object? initialValue = null,
        string? operationName = null)
    {
        var executor = new Executor(schema);
        return executor.Subscribe(new GraphQLRequest
        {
            Document = document,
            InitialValue = initialValue,
            OperationName = operationName,
            Variables = variableValues
        }, cancellationToken);
    }
}