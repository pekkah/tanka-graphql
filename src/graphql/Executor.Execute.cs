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
    public async Task<ExecutionResult> Execute(
        GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var queryContext = BuildQueryContextAsync(request);
        var executionResult = await Execute(queryContext, cancellationToken);

        return executionResult;
    }

    public async Task<ExecutionResult> Execute(
        QueryContext queryContext,
        CancellationToken cancellationToken = default)
    {
        using (_logger.Begin(queryContext.Request.OperationName ?? string.Empty))
        {
            var validationResult = await queryContext.Validate();

            if (!validationResult.IsValid)
                throw new QueryException("todo: validation error")
                {
                    Path = new()
                };

            var executionResult = queryContext.OperationDefinition.Operation switch
            {
                OperationType.Query => await ExecuteQuery(queryContext),
                OperationType.Mutation => await ExecuteQuery(queryContext),
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
        return executor.Execute(new GraphQLRequest
        {
            Document = document,
            InitialValue = initialValue,
            OperationName = operationName,
            Variables = variableValues
        }, cancellationToken);
    }
}