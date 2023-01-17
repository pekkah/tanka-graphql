using System.Runtime.CompilerServices;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

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
    public async Task<ExecutionResult> ExecuteAsync(
        GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryContext = BuildQueryContextAsync(request);
        var executionResult = await ExecuteAsync(queryContext, cancellationToken);

        return executionResult;
    }

    public async Task<ExecutionResult> ExecuteAsync(
        QueryContext queryContext,
        CancellationToken cancellationToken = default)
    {
        using (_logger.Begin(queryContext.Request.OperationName ?? string.Empty))
        {
            //todo: validation
            var validationResult = ValidationResult.Success;

            if (!validationResult.IsValid)
                throw new QueryException("todo: validation error")
                {
                    Path = new()
                };

            var executionResult = queryContext.OperationDefinition.Operation switch
            {
                OperationType.Query => await ExecuteQueryAsync(queryContext),
                OperationType.Mutation => await ExecuteQueryAsync(queryContext),
                OperationType.Subscription => throw new NotImplementedException(),
                _ => throw new InvalidOperationException(
                    $"Operation type {queryContext.OperationDefinition.Operation} not supported.")
            };

            _logger.ExecutionResult(executionResult);
            return executionResult;
        }
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
        return executor.ExecuteAsync(new GraphQLRequest
        {
            Document = document,
            InitialValue = initialValue,
            OperationName = operationName,
            VariableValues = variableValues
        }, cancellationToken);
    }

    /// <summary>
    ///     Execute query or mutation
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ExecutionResult> SubscribeAsync(
        GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryContext = BuildQueryContextAsync(request);
        return SubscribeAsync(queryContext, cancellationToken);
    }

    public async IAsyncEnumerable<ExecutionResult> SubscribeAsync(
        QueryContext queryContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (_logger.Begin(queryContext.Request.OperationName ?? string.Empty))
        {
            //todo: validation
            var validationResult = ValidationResult.Success;

            if (!validationResult.IsValid)
                throw new QueryException("todo: validation error")
                {
                    Path = new()
                };

            switch (queryContext.OperationDefinition.Operation)
            {
                case OperationType.Query:
                    yield return await ExecuteQueryAsync(queryContext);
                    break;
                case OperationType.Mutation:
                    yield return await ExecuteQueryAsync(queryContext);
                    break;
                case OperationType.Subscription:
                    await foreach (var er in ExecuteSubscriptionAsync(queryContext, cancellationToken)) yield return er;
                    break;
            }

            ;
        }
    }
}