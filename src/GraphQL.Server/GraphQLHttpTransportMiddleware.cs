using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Tanka.GraphQL.Request;

namespace Tanka.GraphQL.Server;

public partial class GraphQLHttpTransportMiddleware : IGraphQLRequestMiddleware
{
    private readonly ILogger<GraphQLHttpTransportMiddleware> _logger;

    public GraphQLHttpTransportMiddleware(ILogger<GraphQLHttpTransportMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
    {
        long started = Stopwatch.GetTimestamp();
        HttpContext httpContext = context.HttpContext;

        // Parse request
        GraphQLHttpRequest? request;
        try
        {
            request = await httpContext.Request.ReadFromJsonAsync<GraphQLHttpRequest>();

            if (request is null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Detail = "Could not parse GraphQL request from body of the request"
                });

                Log.NullRequest(_logger);
                return;
            }
        }
        catch (Exception x)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Could not parse GraphQL request from body of the request",
                Detail = x.Message // could this leak?
            });

            Log.RequestParseError(_logger, x);
            return;
        }


        Log.HttpRequest(_logger, request);

        context.Request = new GraphQLRequest
        {
            InitialValue = null,
            Document = request.Query,
            OperationName = request.OperationName,
            Variables = request.Variables
        };

        await next(context);

        await using IAsyncEnumerator<ExecutionResult> enumerator =
            context.Response.GetAsyncEnumerator(context.RequestCancelled);

        if (await enumerator.MoveNextAsync())
        {
            ExecutionResult initialResult = enumerator.Current;

            if (await enumerator.MoveNextAsync())
            {
                Log.MultipleResultsError(_logger);
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "HttpTransport does not support multiple execution results"
                });

                return;
            }

            string elapsed = $"{Stopwatch.GetElapsedTime(started).TotalMilliseconds}ms";
            httpContext.Response.Headers["Elapsed"] = new StringValues(elapsed);
            await httpContext.Response.WriteAsJsonAsync(initialResult);
            Log.ExecutionResult(_logger, initialResult, elapsed);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(5, LogLevel.Debug, "Result({elapsed}): {result}")]
        public static partial void ExecutionResult(ILogger logger, ExecutionResult? result, string elapsed);

        [LoggerMessage(3, LogLevel.Debug, "Request: {request}")]
        public static partial void HttpRequest(ILogger logger, GraphQLHttpRequest request);

        [LoggerMessage(3003, LogLevel.Error, "HttpTransport does not support multiple execution results.")]
        public static partial void MultipleResultsError(ILogger logger);

        [LoggerMessage(3001, LogLevel.Error, "Could not parse GraphQL HTTP request. Request is empty.")]
        public static partial void NullRequest(ILogger logger);

        [LoggerMessage(3002, LogLevel.Error, "Could not parse GraphQL HTTP request. Error while parsing json.")]
        public static partial void RequestParseError(ILogger logger, Exception x);
    }
}