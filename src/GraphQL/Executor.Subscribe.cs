using System.Runtime.CompilerServices;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL;

public partial class Executor
{
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
        }
    }
}