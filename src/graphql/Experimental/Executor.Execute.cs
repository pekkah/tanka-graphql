﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;

namespace Tanka.GraphQL.Experimental;

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
                    Path = new NodePath()
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
}