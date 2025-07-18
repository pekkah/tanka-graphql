using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tanka.GraphQL.Server;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLHttpTransportFacts
{
    [Fact]
    public void Constructor_ShouldInitializeWithLogger()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        
        // When
        var transport = new GraphQLHttpTransport(logger);
        
        // Then
        Assert.NotNull(transport);
    }

    [Fact]
    public void Map_ShouldMapPostAndGetRoutes()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var routeBuilder = Substitute.For<IEndpointRouteBuilder>();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var pattern = "/graphql";
        var postBuilder = Substitute.For<IEndpointConventionBuilder>();
        var getBuilder = Substitute.For<IEndpointConventionBuilder>();
        
        routeBuilder.MapPost(pattern, Arg.Any<RequestDelegate>()).Returns(postBuilder);
        routeBuilder.MapGet(pattern, Arg.Any<RequestDelegate>()).Returns(getBuilder);
        
        // When
        var result = transport.Map(pattern, routeBuilder, requestDelegate);
        
        // Then
        Assert.NotNull(result);
        routeBuilder.Received().MapPost(pattern, Arg.Any<RequestDelegate>());
        routeBuilder.Received().MapGet(pattern, Arg.Any<RequestDelegate>());
    }

    [Fact]
    public void Build_ShouldUseHttpTransport()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var builder = Substitute.For<GraphQLRequestPipelineBuilder>();
        
        // When
        transport.Build(builder);
        
        // Then
        builder.Received().UseHttpTransport();
    }

    [Fact]
    public async Task ProcessRequest_ShouldHandleValidJsonRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.True(processingCompleted);
    }

    [Fact]
    public async Task ProcessRequest_ShouldSkipWebSocketRequests()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.WebSockets.IsWebSocketRequest.Returns(true);
        httpContext.Request.ContentType = "application/json";
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.False(processingCompleted);
    }

    [Fact]
    public async Task ProcessRequest_ShouldSkipNonJsonRequests()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.Request.ContentType = "text/plain";
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.False(processingCompleted);
    }

    [Fact]
    public async Task ProcessRequest_ShouldSetHttpContextFeature()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        GraphQLRequestContext capturedContext = null;
        
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => capturedContext = x.Arg<GraphQLRequestContext>());
        
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.NotNull(capturedContext);
        var httpContextFeature = capturedContext.Features.Get<IHttpContextFeature>();
        Assert.NotNull(httpContextFeature);
        Assert.Equal(httpContext, httpContextFeature.HttpContext);
    }

    [Fact]
    public async Task ProcessRequest_ShouldSetRequestServices()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        GraphQLRequestContext capturedContext = null;
        
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => capturedContext = x.Arg<GraphQLRequestContext>());
        
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.NotNull(capturedContext);
        Assert.Equal(httpContext.RequestServices, capturedContext.RequestServices);
        Assert.Equal(httpContext.RequestAborted, capturedContext.RequestCancelled);
    }

    [Fact]
    public async Task ProcessRequest_ShouldHandleCancellation()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var cts = new CancellationTokenSource();
        GraphQLRequestContext capturedContext = null;
        
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => capturedContext = x.Arg<GraphQLRequestContext>());
        
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        httpContext.RequestAborted.Returns(cts.Token);
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.NotNull(capturedContext);
        Assert.Equal(cts.Token, capturedContext.RequestCancelled);
    }

    [Fact]
    public async Task ProcessRequest_ShouldLogBeginAndEndRequest()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        logger.Received().LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessRequest_ShouldHandleEmptyContentType()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.Request.ContentType = "";
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.False(processingCompleted);
    }

    [Fact]
    public async Task ProcessRequest_ShouldHandleNullContentType()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.Request.ContentType = null;
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.False(processingCompleted);
    }

    [Fact]
    public async Task ProcessRequest_ShouldHandleApplicationJsonWithCharset()
    {
        // Given
        var logger = Substitute.For<ILogger<GraphQLHttpTransport>>();
        var transport = new GraphQLHttpTransport(logger);
        var httpContext = CreateHttpContext();
        var requestDelegate = Substitute.For<GraphQLRequestDelegate>();
        var processingCompleted = false;
        
        requestDelegate.When(x => x(Arg.Any<GraphQLRequestContext>()))
            .Do(x => processingCompleted = true);
        
        httpContext.Request.ContentType = "application/json; charset=utf-8";
        httpContext.Request.Body = CreateJsonRequestStream("{ hello }");
        
        // When
        var processMethod = typeof(GraphQLHttpTransport)
            .GetMethod("ProcessRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var requestHandler = (RequestDelegate)processMethod.Invoke(transport, new object[] { requestDelegate });
        
        await requestHandler(httpContext);
        
        // Then
        Assert.True(processingCompleted);
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var response = Substitute.For<HttpResponse>();
        var webSockets = Substitute.For<WebSocketManager>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        
        httpContext.Request.Returns(request);
        httpContext.Response.Returns(response);
        httpContext.WebSockets.Returns(webSockets);
        httpContext.RequestServices.Returns(serviceProvider);
        httpContext.RequestAborted.Returns(CancellationToken.None);
        
        webSockets.IsWebSocketRequest.Returns(false);
        
        return httpContext;
    }

    private static System.IO.Stream CreateJsonRequestStream(string query)
    {
        var requestJson = $@"{{""query"": ""{query}""}}";
        var bytes = Encoding.UTF8.GetBytes(requestJson);
        return new System.IO.MemoryStream(bytes);
    }
}