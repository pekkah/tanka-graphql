using System.Diagnostics;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Tanka.GraphQL.Request;

namespace Tanka.GraphQL.Server;

public partial class GraphQLHttpTransportMiddleware(ILogger<GraphQLHttpTransportMiddleware> logger)
    : IGraphQLRequestMiddleware
{
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

                Log.NullRequest(logger);
                return;
            }
        }
        catch (Exception x)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Could not parse GraphQL request from body of the request",
                Detail = "Invalid request format or content"
            });

            Log.RequestParseError(logger, x);
            return;
        }


        Log.HttpRequest(logger, request);

        context.Request = new GraphQLRequest
        {
            InitialValue = null,
            Query = request.Query,
            OperationName = request.OperationName,
            Variables = request.Variables
        };

        await next(context);

        await using IAsyncEnumerator<ExecutionResult> enumerator =
            context.Response.GetAsyncEnumerator(context.RequestCancelled);

        if (await enumerator.MoveNextAsync())
        {
            ExecutionResult firstResult = enumerator.Current;

            // Check if there are more results
            if (await enumerator.MoveNextAsync())
            {
                if (SupportsMultipart(httpContext.Request, logger))
                {
                    Log.MultipartStreamingStarted(logger);
                    // Stream all results using the existing enumerator
                    await WriteMultipartStreamingResponse(httpContext.Response, firstResult, enumerator, context.RequestCancelled, logger);
                    Log.MultipartStreamingCompleted(logger);
                }
                else
                {
                    // Legacy behavior - reject multiple results
                    Log.MultipleResultsError(logger);
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Title = "HttpTransport does not support multiple execution results"
                    });
                }
                return;
            }

            // Single result - normal response
            await WriteSingleResponse(httpContext.Response, firstResult, started);
        }
    }

    internal static bool SupportsMultipart(HttpRequest request, ILogger<GraphQLHttpTransportMiddleware> logger)
    {
        // Check Accept header for multipart/mixed support
        var acceptHeader = request.Headers.Accept.ToString();
        bool supportsMultipart = acceptHeader.Contains("multipart/mixed", StringComparison.OrdinalIgnoreCase) ||
                                acceptHeader.Contains("deferSpec=20220824", StringComparison.OrdinalIgnoreCase);

        Log.MultipartSupportDetection(logger, supportsMultipart, acceptHeader);
        return supportsMultipart;
    }

    private static async Task WriteSingleResponse(HttpResponse httpResponse, ExecutionResult result, long started)
    {
        string elapsed = $"{Stopwatch.GetElapsedTime(started).TotalMilliseconds}ms";
        httpResponse.Headers["Elapsed"] = new StringValues(elapsed);
        await httpResponse.WriteAsJsonAsync(result);
    }

    internal static async Task WriteMultipartStreamingResponse(
        HttpResponse httpResponse,
        ExecutionResult firstResult,
        IAsyncEnumerator<ExecutionResult> enumerator,
        CancellationToken cancellationToken,
        ILogger<GraphQLHttpTransportMiddleware> logger)
    {
        const string boundary = "graphql-response";
        httpResponse.ContentType = $"multipart/mixed; boundary={boundary}";

        // Let ASP.NET Core handle chunked encoding automatically - don't set it manually

        await using var writer = new StreamWriter(httpResponse.Body);

        int chunkCount = 0;
        var streamingStarted = Stopwatch.GetTimestamp();

        // Write first result with hasNext = true (no data generation time since it's already available)
        chunkCount++;
        var chunkTotalStarted = Stopwatch.GetTimestamp();
        var chunkStarted = Stopwatch.GetTimestamp();
        Log.MultipartChunkWriting(logger, chunkCount, hasNext: true);
        await WriteMultipartChunk(httpResponse, writer, boundary, firstResult, hasNext: true, cancellationToken);
        await writer.FlushAsync(cancellationToken); // Flush immediately for streaming
        var chunkElapsed = Stopwatch.GetElapsedTime(chunkStarted);
        var chunkTotalElapsed = Stopwatch.GetElapsedTime(chunkTotalStarted);
        Log.MultipartChunkSerialized(logger, chunkCount, chunkElapsed.TotalMilliseconds);
        Log.MultipartChunkTotalTime(logger, chunkCount, chunkTotalElapsed.TotalMilliseconds);

        // Stream remaining results (enumerator is already positioned at second result)
        var current = enumerator.Current;

        while (true)
        {
            // Start timing the entire chunk (data generation + serialization)
            chunkTotalStarted = Stopwatch.GetTimestamp();

            // Measure time to generate next result (the @defer data generation)
            var dataGenerationStarted = Stopwatch.GetTimestamp();
            bool hasMoreData = await enumerator.MoveNextAsync();
            if (!hasMoreData) break;

            var dataGenerationElapsed = Stopwatch.GetElapsedTime(dataGenerationStarted);
            Log.MultipartDataGenerated(logger, chunkCount + 1, dataGenerationElapsed.TotalMilliseconds);

            // Write current result with hasNext = true
            chunkCount++;
            chunkStarted = Stopwatch.GetTimestamp();
            Log.MultipartChunkWriting(logger, chunkCount, hasNext: true);
            await WriteMultipartChunk(httpResponse, writer, boundary, current, hasNext: true, cancellationToken);
            await writer.FlushAsync(cancellationToken); // Flush immediately for streaming
            chunkElapsed = Stopwatch.GetElapsedTime(chunkStarted);
            chunkTotalElapsed = Stopwatch.GetElapsedTime(chunkTotalStarted);
            Log.MultipartChunkSerialized(logger, chunkCount, chunkElapsed.TotalMilliseconds);
            Log.MultipartChunkTotalTime(logger, chunkCount, chunkTotalElapsed.TotalMilliseconds);
            current = enumerator.Current;
        }

        // Write final result with hasNext = false
        chunkCount++;
        chunkTotalStarted = Stopwatch.GetTimestamp();
        chunkStarted = Stopwatch.GetTimestamp();
        Log.MultipartChunkWriting(logger, chunkCount, hasNext: false);
        await WriteMultipartChunk(httpResponse, writer, boundary, current, hasNext: false, cancellationToken);
        chunkElapsed = Stopwatch.GetElapsedTime(chunkStarted);
        chunkTotalElapsed = Stopwatch.GetElapsedTime(chunkTotalStarted);
        Log.MultipartChunkSerialized(logger, chunkCount, chunkElapsed.TotalMilliseconds);
        Log.MultipartChunkTotalTime(logger, chunkCount, chunkTotalElapsed.TotalMilliseconds);

        // Close multipart boundary
        await writer.WriteAsync($"\r\n--{boundary}--\r\n");
        await writer.FlushAsync();

        var totalElapsed = Stopwatch.GetElapsedTime(streamingStarted);
        Log.MultipartStreamingStats(logger, chunkCount, totalElapsed.TotalMilliseconds);
    }

    private static async Task WriteMultipartChunk(
        HttpResponse httpResponse,
        StreamWriter writer,
        string boundary,
        ExecutionResult result,
        bool hasNext,
        CancellationToken cancellationToken)
    {
        await writer.WriteAsync($"\r\n--{boundary}\r\n");
        await writer.WriteAsync("Content-Type: application/json; charset=utf-8\r\n\r\n");

        // Ensure hasNext is set correctly
        var responseWithNext = result with { HasNext = hasNext };

        // Use a memory stream to capture JSON serialization with HttpResponse's configured options
        using var memoryStream = new MemoryStream();
        var originalBody = httpResponse.Body;
        httpResponse.Body = memoryStream;

        await httpResponse.WriteAsJsonAsync(responseWithNext, cancellationToken);

        httpResponse.Body = originalBody;
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream);
        var json = await reader.ReadToEndAsync();
        await writer.WriteAsync(json);
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

        [LoggerMessage(4001, LogLevel.Information, "Starting multipart streaming response for @defer/@stream")]
        public static partial void MultipartStreamingStarted(ILogger logger);

        [LoggerMessage(4002, LogLevel.Information, "Multipart streaming response completed successfully")]
        public static partial void MultipartStreamingCompleted(ILogger logger);

        [LoggerMessage(4003, LogLevel.Debug, "Writing multipart chunk {chunkNumber} (hasNext: {hasNext})")]
        public static partial void MultipartChunkWriting(ILogger logger, int chunkNumber, bool hasNext);

        [LoggerMessage(4004, LogLevel.Information, "Multipart streaming completed: {totalChunks} chunks sent in {totalElapsedMs}ms")]
        public static partial void MultipartStreamingStats(ILogger logger, int totalChunks, double totalElapsedMs);

        [LoggerMessage(4005, LogLevel.Debug, "Multipart support detection: {supportsMultipart} (Accept: {acceptHeader})")]
        public static partial void MultipartSupportDetection(ILogger logger, bool supportsMultipart, string acceptHeader);

        [LoggerMessage(4006, LogLevel.Debug, "Multipart chunk {chunkNumber} serialized in {elapsedMs}ms")]
        public static partial void MultipartChunkSerialized(ILogger logger, int chunkNumber, double elapsedMs);

        [LoggerMessage(4007, LogLevel.Information, "@defer data for chunk {chunkNumber} generated in {elapsedMs}ms")]
        public static partial void MultipartDataGenerated(ILogger logger, int chunkNumber, double elapsedMs);

        [LoggerMessage(4008, LogLevel.Information, "Chunk {chunkNumber} total time: {elapsedMs}ms")]
        public static partial void MultipartChunkTotalTime(ILogger logger, int chunkNumber, double elapsedMs);
    }
}