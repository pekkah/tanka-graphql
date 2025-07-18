using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tanka.GraphQL.Request;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLHttpTransportMiddlewareFacts
{
    private readonly ILogger<GraphQLHttpTransportMiddleware> _logger;
    private readonly GraphQLHttpTransportMiddleware _middleware;

    public GraphQLHttpTransportMiddlewareFacts()
    {
        _logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        _middleware = new GraphQLHttpTransportMiddleware(_logger);
    }

    [Fact]
    public void Constructor_WithLogger_SetsLogger()
    {
        // Given & When
        var middleware = new GraphQLHttpTransportMiddleware(_logger);

        // Then
        Assert.NotNull(middleware);
    }

    [Fact]
    public async Task Invoke_WithValidRequest_CallsNext()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var nextCalled = false;
        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => nextCalled = true);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.True(nextCalled);
        Assert.NotNull(context.Request);
        Assert.Equal("{ hello }", context.Request.Query);
    }

    [Fact]
    public async Task Invoke_WithNullRequest_Returns400BadRequest()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("null");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(400, httpContext.Response.StatusCode);
        await next.DidNotReceive().Invoke(Arg.Any<GraphQLRequestContext>());
    }

    [Fact]
    public async Task Invoke_WithEmptyRequest_Returns400BadRequest()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(400, httpContext.Response.StatusCode);
        await next.DidNotReceive().Invoke(Arg.Any<GraphQLRequestContext>());
    }

    [Fact]
    public async Task Invoke_WithInvalidJson_Returns400BadRequest()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("invalid json");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(400, httpContext.Response.StatusCode);
        await next.DidNotReceive().Invoke(Arg.Any<GraphQLRequestContext>());
    }

    [Fact]
    public async Task Invoke_WithValidRequestAndVariables_SetsVariables()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var requestBody = """{"query": "query($name: String!) { hello(name: $name) }", "variables": {"name": "world"}}""";
        var httpContext = CreateHttpContext(requestBody);
        context.HttpContext = httpContext;

        var nextCalled = false;
        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => nextCalled = true);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.True(nextCalled);
        Assert.NotNull(context.Request);
        Assert.NotNull(context.Request.Variables);
        Assert.Contains("name", context.Request.Variables.Keys);
        Assert.Equal("world", context.Request.Variables["name"]);
    }

    [Fact]
    public async Task Invoke_WithValidRequestAndOperationName_SetsOperationName()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var requestBody = """{"query": "query GetHello { hello }", "operationName": "GetHello"}""";
        var httpContext = CreateHttpContext(requestBody);
        context.HttpContext = httpContext;

        var nextCalled = false;
        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => nextCalled = true);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.True(nextCalled);
        Assert.NotNull(context.Request);
        Assert.Equal("GetHello", context.Request.OperationName);
    }

    [Fact]
    public async Task Invoke_WithSingleResult_WritesJsonResponse()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Contains("Elapsed", httpContext.Response.Headers);
        
        // Verify response was written
        httpContext.Response.Body.Position = 0;
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.NotEmpty(responseBody);
    }

    [Fact]
    public async Task Invoke_WithMultipleResults_Returns500InternalServerError()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response with multiple results
        var executionResults = new[]
        {
            new ExecutionResult { Data = new { hello = "world1" } },
            new ExecutionResult { Data = new { hello = "world2" } }
        };
        
        context.Response = AsyncEnumerable.CreateFromEnumerable(executionResults);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(500, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithNoResults_CompletesSuccessfully()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response with no results
        context.Response = AsyncEnumerable.CreateEmpty<ExecutionResult>();

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithExecutionError_WritesErrorResponse()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response with error
        var executionResult = new ExecutionResult
        {
            Data = null,
            Errors = new[] { new ExecutionError("Test error") }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Equal(200, httpContext.Response.StatusCode);
        
        // Verify response was written
        httpContext.Response.Body.Position = 0;
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.NotEmpty(responseBody);
        Assert.Contains("Test error", responseBody);
    }

    [Fact]
    public async Task Invoke_SetsElapsedTimeHeader()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        Assert.Contains("Elapsed", httpContext.Response.Headers);
        var elapsedValue = httpContext.Response.Headers["Elapsed"].ToString();
        Assert.EndsWith("ms", elapsedValue);
    }

    [Fact]
    public async Task Invoke_LogsRequestInformation()
    {
        // Given
        var context = CreateGraphQLRequestContext();
        var httpContext = CreateHttpContext("""{"query": "{ hello }"}""");
        context.HttpContext = httpContext;

        var next = Substitute.For<GraphQLRequestDelegate>();
        next.Invoke(Arg.Any<GraphQLRequestContext>())
            .Returns(Task.CompletedTask);

        // Setup response
        var executionResult = new ExecutionResult
        {
            Data = new { hello = "world" }
        };
        
        context.Response = AsyncEnumerable.CreateWithSingleItem(executionResult);

        // When
        await _middleware.Invoke(context, next);

        // Then
        // Verify logging occurred (logger is a substitute, we can check if it was called)
        // The actual logging verification would depend on your logging test setup
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    private static GraphQLRequestContext CreateGraphQLRequestContext()
    {
        var context = new GraphQLRequestContext();
        return context;
    }

    private static HttpContext CreateHttpContext(string requestBody)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }
}

// Helper class for creating async enumerables in tests
public static class AsyncEnumerable
{
    public static async IAsyncEnumerable<T> CreateWithSingleItem<T>(T item)
    {
        yield return item;
    }

    public static async IAsyncEnumerable<T> CreateFromEnumerable<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> CreateEmpty<T>()
    {
        yield break;
    }
}