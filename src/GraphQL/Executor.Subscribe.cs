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
    public IAsyncEnumerable<ExecutionResult> Subscribe(
        GraphQLRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        QueryContext queryContext = BuildQueryContextAsync(request);
        return Subscribe(queryContext, cancellationToken);
    }

    public async IAsyncEnumerable<ExecutionResult> Subscribe(
        QueryContext queryContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (_logger.Begin(queryContext.Request.OperationName ?? string.Empty))
        {
            ValidationResult validationResult = await queryContext.Validate();

            if (!validationResult.IsValid)
                throw new QueryException(validationResult.Errors.First().Message)
                {
                    Path = new NodePath()
                };

            switch (queryContext.OperationDefinition.Operation)
            {
                case OperationType.Query:
                    yield return await ExecuteQuery(queryContext);
                    break;
                case OperationType.Mutation:
                    yield return await ExecuteQuery(queryContext);
                    break;
                case OperationType.Subscription:
                    await foreach (ExecutionResult er in ExecuteSubscription(queryContext, cancellationToken))
                        yield return er;
                    break;
            }
        }
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