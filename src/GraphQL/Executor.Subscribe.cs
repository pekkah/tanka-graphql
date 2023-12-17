using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;

namespace Tanka.GraphQL;

public partial class Executor
{
    /// <summary>
    ///     Execute subscription operation with given <paramref name="request"/>.
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


    /// <summary>
    ///     Static method for executing subscription operation with given <paramref name="request"/> and defaults.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="document"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="variableValues"></param>
    /// <param name="initialValue"></param>
    /// <param name="operationName"></param>
    /// <returns></returns>
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