using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.Server;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLHttpTransportMiddlewareFacts
{
    [Fact]
    public async Task Invoke_ShouldParseValidJsonRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;
        var capturedContext = (GraphQLRequestContext?)null;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => { nextCalled = true; capturedContext = x.Arg<GraphQLRequestContext>(); });

        var requestJson = @"{""query"": ""{ hello }"", ""operationName"": ""Test"", ""variables"": {""id"": ""123""}}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
        Assert.NotNull(capturedContext);
        Assert.NotNull(capturedContext.Request);
        Assert.Equal("{ hello }", capturedContext.Request.Query);
        Assert.Equal("Test", capturedContext.Request.OperationName);
        Assert.NotNull(capturedContext.Request.Variables);
    }

    [Fact]
    public async Task Invoke_ShouldHandle400OnNullRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => nextCalled = true);

        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("null"));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        httpContext.Response.Received().WriteAsJsonAsync(Arg.Any<ProblemDetails>());
    }

    [Fact]
    public async Task Invoke_ShouldHandle400OnInvalidJson()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => nextCalled = true);

        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        httpContext.Response.Received().WriteAsJsonAsync(Arg.Any<ProblemDetails>());
    }

    [Fact]
    public async Task Invoke_ShouldHandle500OnMultipleResults()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                nextCalled = true;
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateMultipleResultsAsyncEnumerable();
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        httpContext.Response.Received().WriteAsJsonAsync(Arg.Any<ProblemDetails>());
    }

    [Fact]
    public async Task Invoke_ShouldWriteExecutionResultAndElapsedTime()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;
        var executionResult = new ExecutionResult
        {
            Data = new Dictionary<string, object> { { "hello", "world" } }
        };

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                nextCalled = true;
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateSingleResultAsyncEnumerable(executionResult);
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
        httpContext.Response.Received().WriteAsJsonAsync(executionResult);
        httpContext.Response.Headers.Received().Add("Elapsed", Arg.Any<StringValues>());
    }

    [Fact]
    public async Task Invoke_ShouldHandleEmptyResponse()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                nextCalled = true;
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateEmptyAsyncEnumerable();
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
        httpContext.Response.DidNotReceive().WriteAsJsonAsync(Arg.Any<ExecutionResult>());
    }

    [Fact]
    public async Task Invoke_ShouldHandleRequestCancellation()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var cts = new CancellationTokenSource();
        var context = CreateGraphQLRequestContext(httpContext, cts.Token);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                nextCalled = true;
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateCancellableAsyncEnumerable(cts.Token);
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        cts.Cancel();
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Invoke_ShouldLogRequestParseError()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        logger.Received().LogError(Arg.Any<string>(), Arg.Any<Exception>());
    }

    [Fact]
    public async Task Invoke_ShouldLogNullRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("null"));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        logger.Received().LogError(Arg.Any<string>());
    }

    [Fact]
    public async Task Invoke_ShouldLogMultipleResultsError()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateMultipleResultsAsyncEnumerable();
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        logger.Received().LogError(Arg.Any<string>());
    }

    [Fact]
    public async Task Invoke_ShouldLogHttpRequestAndExecutionResult()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var executionResult = new ExecutionResult();

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => {
                var ctx = x.Arg<GraphQLRequestContext>();
                ctx.Response = CreateSingleResultAsyncEnumerable(executionResult);
            });

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        logger.Received().LogDebug(Arg.Any<string>(), Arg.Any<GraphQLHttpRequest>());
        logger.Received().LogDebug(Arg.Any<string>(), Arg.Any<ExecutionResult>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Invoke_ShouldHandleMinimalValidRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => nextCalled = true);

        var requestJson = @"{""query"": ""{ hello }""}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Invoke_ShouldHandleEmptyVariables()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransportMiddleware>>();
        var middleware = new GraphQLHttpTransportMiddleware(logger);
        var httpContext = CreateHttpContext();
        var context = CreateGraphQLRequestContext(httpContext);
        var nextCalled = false;
        var capturedContext = (GraphQLRequestContext?)null;

        var nextDelegate = Substitute.For<GraphQLRequestDelegate>();
        nextDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => { nextCalled = true; capturedContext = x.Arg<GraphQLRequestContext>(); });

        var requestJson = @"{""query"": ""{ hello }"", ""variables"": {}}";
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestJson));

        // When
        await middleware.Invoke(context, nextDelegate);

        // Then
        Assert.True(nextCalled);
        Assert.NotNull(capturedContext);
        Assert.NotNull(capturedContext.Request);
        Assert.NotNull(capturedContext.Request.Variables);
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var response = Substitute.For<HttpResponse>();
        var headers = Substitute.For<IHeaderDictionary>();
        
        httpContext.Request.Returns(request);
        httpContext.Response.Returns(response);
        response.Headers.Returns(headers);
        
        return httpContext;
    }

    private static GraphQLRequestContext CreateGraphQLRequestContext(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var context = new GraphQLRequestContext
        {
            RequestCancelled = cancellationToken
        };
        
        context.Features.Set<IHttpContextFeature>(new HttpContextFeature
        {
            HttpContext = httpContext
        });
        
        return context;
    }

    private static IAsyncEnumerable<ExecutionResult> CreateSingleResultAsyncEnumerable(ExecutionResult result)
    {
        var enumerable = Substitute.For<IAsyncEnumerable<ExecutionResult>>();
        var enumerator = Substitute.For<IAsyncEnumerator<ExecutionResult>>();
        
        enumerable.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(enumerator);
        
        enumerator.MoveNextAsync().Returns(
            new ValueTask<bool>(true),  // First call returns true
            new ValueTask<bool>(false)  // Second call returns false
        );
        enumerator.Current.Returns(result);
        
        return enumerable;
    }

    private static IAsyncEnumerable<ExecutionResult> CreateMultipleResultsAsyncEnumerable()
    {
        var enumerable = Substitute.For<IAsyncEnumerable<ExecutionResult>>();
        var enumerator = Substitute.For<IAsyncEnumerator<ExecutionResult>>();
        
        enumerable.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(enumerator);
        
        enumerator.MoveNextAsync().Returns(
            new ValueTask<bool>(true),  // First call returns true
            new ValueTask<bool>(true)   // Second call returns true (multiple results)
        );
        enumerator.Current.Returns(new ExecutionResult());
        
        return enumerable;
    }

    private static IAsyncEnumerable<ExecutionResult> CreateEmptyAsyncEnumerable()
    {
        var enumerable = Substitute.For<IAsyncEnumerable<ExecutionResult>>();
        var enumerator = Substitute.For<IAsyncEnumerator<ExecutionResult>>();
        
        enumerable.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(enumerator);
        enumerator.MoveNextAsync().Returns(new ValueTask<bool>(false));
        
        return enumerable;
    }

    private static IAsyncEnumerable<ExecutionResult> CreateCancellableAsyncEnumerable(CancellationToken cancellationToken)
    {
        var enumerable = Substitute.For<IAsyncEnumerable<ExecutionResult>>();
        var enumerator = Substitute.For<IAsyncEnumerator<ExecutionResult>>();
        
        enumerable.GetAsyncEnumerator(cancellationToken).Returns(enumerator);
        enumerator.MoveNextAsync().Returns(new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken)));
        
        return enumerable;
    }
}