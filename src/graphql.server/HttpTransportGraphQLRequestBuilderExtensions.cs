using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tanka.GraphQL.Server;

public static class HttpTransportGraphQLRequestBuilderExtensions {

    public static GraphQLRequestPipelineBuilder UseHttpTransport(this GraphQLRequestPipelineBuilder builder)
    {
        return builder.Use(next => async context =>
        {
            var stopwatch = Stopwatch.StartNew();
            var httpContext = context.HttpContext;

            // Parse request
            var request = await httpContext.Request.ReadFromJsonAsync<GraphQLHttpRequest>();

            if (request is null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Detail = "Could not parse GraphQL request from body of the request"
                });

                return;
            }

            context.Request = new GraphQLRequest
            {
                InitialValue = null,
                Document = request.Query,
                OperationName = request.OperationName,
                Variables = request.Variables
            };

            await next(context);

            await using var enumerator = context.Response.GetAsyncEnumerator(context.RequestCancelled);

            if (await enumerator.MoveNextAsync())
            {
                var initialResult = enumerator.Current;

                if (await enumerator.MoveNextAsync())
                    throw new InvalidOperationException("HttpTransport does not support multiple responses.");

                stopwatch.Stop();
                httpContext.Response.Headers["Elapsed"] = new($"{stopwatch.Elapsed.TotalSeconds}s");
                await httpContext.Response.WriteAsJsonAsync(initialResult);
            }
        });
    }
}