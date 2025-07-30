using System.Collections.Generic;
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
        if (document == null)
            throw new ArgumentNullException(nameof(document));
            
        var executor = new Executor(schema);
        return executor.Execute(new GraphQLRequest
        {
            Query = document,
            InitialValue = initialValue,
            OperationName = operationName,
            Variables = variableValues
        }, cancellationToken);
    }

    /// <summary>
    ///     Static execute method for executing query or mutation operation from a string query.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="query"></param>
    /// <param name="variableValues"></param>
    /// <param name="initialValue"></param>
    /// <param name="operationName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<ExecutionResult> Execute(
        ISchema schema,
        string query,
        Dictionary<string, object?>? variableValues = null,
        object? initialValue = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        // Handle empty or whitespace-only queries
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ExecutionResult
            {
                Errors = new List<ExecutionError>
                {
                    new ExecutionError
                    {
                        Message = "Query cannot be empty",
                        Extensions = new Dictionary<string, object>
                        {
                            ["code"] = "EMPTY_QUERY"
                        }
                    }
                }
            };
        }

        try
        {
            ExecutableDocument document = query; // Implicit conversion
            return await Execute(schema, document, variableValues, initialValue, operationName, cancellationToken);
        }
        catch (Exception ex) when (ex.Message.Contains("Unexpected token") || ex.Message.Contains("Expected:") || ex.Message.Contains("Unexpected end"))
        {
            // Parser syntax errors should be returned as errors in the result
            return new ExecutionResult
            {
                Errors = new List<ExecutionError>
                {
                    new ExecutionError
                    {
                        Message = ex.Message,
                        Extensions = new Dictionary<string, object>
                        {
                            ["code"] = "SYNTAX_ERROR"
                        }
                    }
                }
            };
        }
    }
}