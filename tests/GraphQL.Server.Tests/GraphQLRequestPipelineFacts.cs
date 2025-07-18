using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Tanka.GraphQL.Request;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLRequestPipelineFacts
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GraphQLRequestPipelineBuilder> _logger;

    public GraphQLRequestPipelineFacts()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<GraphQLRequestPipelineBuilder>>();
    }

    [Fact]
    public void Build_EmptyPipeline_ReturnsNullDelegate()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);

        /* When */
        var pipeline = builder.Build();

        /* Then */
        Assert.Null(pipeline);
    }

    [Fact]
    public void Use_SingleMiddleware_BuildsCorrectly()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var middlewareInvoked = false;

        /* When */
        builder.Use(async (context, next) =>
        {
            middlewareInvoked = true;
            await next(context);
        });

        var pipeline = builder.Build();

        /* Then */
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task Use_SingleMiddleware_InvokesMiddleware()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var middlewareInvoked = false;

        builder.Use(async (context, next) =>
        {
            middlewareInvoked = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.True(middlewareInvoked);
    }

    [Fact]
    public async Task Use_MultipleMiddleware_InvokesInCorrectOrder()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var invocationOrder = new List<string>();

        builder.Use(async (context, next) =>
        {
            invocationOrder.Add("First-Before");
            await next(context);
            invocationOrder.Add("First-After");
        });

        builder.Use(async (context, next) =>
        {
            invocationOrder.Add("Second-Before");
            await next(context);
            invocationOrder.Add("Second-After");
        });

        builder.Use(async (context, next) =>
        {
            invocationOrder.Add("Third-Before");
            await next(context);
            invocationOrder.Add("Third-After");
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.Equal(new[] { "First-Before", "Second-Before", "Third-Before", "Third-After", "Second-After", "First-After" }, invocationOrder);
    }

    [Fact]
    public async Task Use_MiddlewareThrowsException_PropagatesException()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var expectedException = new InvalidOperationException("Test exception");

        builder.Use(async (context, next) =>
        {
            await next(context);
        });

        builder.Use(async (context, next) =>
        {
            throw expectedException;
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline!(context));

        /* Then */
        Assert.Equal(expectedException, exception);
    }

    [Fact]
    public async Task Use_MiddlewareDoesNotCallNext_StopsExecution()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var secondMiddlewareInvoked = false;

        builder.Use(async (context, next) =>
        {
            // Don't call next - should stop execution
            await Task.CompletedTask;
        });

        builder.Use(async (context, next) =>
        {
            secondMiddlewareInvoked = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.False(secondMiddlewareInvoked);
    }

    [Fact]
    public void UseMiddleware_WithType_AddsMiddleware()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);

        /* When */
        builder.UseMiddleware<TestMiddleware>();
        var pipeline = builder.Build();

        /* Then */
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task UseMiddleware_WithType_InvokesMiddleware()
    {
        /* Given */
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<TestMiddleware>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<GraphQLRequestPipelineBuilder>>();

        var builder = new GraphQLRequestPipelineBuilder(serviceProvider, logger);

        builder.UseMiddleware<TestMiddleware>();
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        var middleware = serviceProvider.GetRequiredService<TestMiddleware>();
        Assert.True(middleware.WasInvoked);
    }

    [Fact]
    public async Task UseMiddleware_WithArgs_PassesArgumentsCorrectly()
    {
        /* Given */
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<GraphQLRequestPipelineBuilder>>();

        var builder = new GraphQLRequestPipelineBuilder(serviceProvider, logger);

        var testValue = "test123";
        builder.UseMiddleware<TestMiddlewareWithArgs>(testValue);
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.Equal(testValue, context.Features.Get<string>());
    }

    [Fact]
    public async Task Pipeline_ModifiesContext_ChangesAreVisible()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var testValue = "modified";

        builder.Use(async (context, next) =>
        {
            context.Features.Set(testValue);
            await next(context);
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.Equal(testValue, context.Features.Get<string>());
    }

    [Fact]
    public async Task Pipeline_WithAsyncMiddleware_WorksCorrectly()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var delayComplete = false;

        builder.Use(async (context, next) =>
        {
            await Task.Delay(10);
            delayComplete = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext { RequestServices = _serviceProvider };

        /* When */
        await pipeline!(context);

        /* Then */
        Assert.True(delayComplete);
    }

    [Fact]
    public async Task Pipeline_WithCancellation_RespondsCorrectly()
    {
        /* Given */
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider, _logger);
        var cancellationTokenSource = new CancellationTokenSource();
        var middlewareCompleted = false;

        builder.Use(async (context, next) =>
        {
            await Task.Delay(50, context.RequestCancelled);
            middlewareCompleted = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var context = new GraphQLRequestContext 
        { 
            RequestServices = _serviceProvider,
            RequestCancelled = cancellationTokenSource.Token
        };

        /* When */
        cancellationTokenSource.Cancel();

        /* Then */
        await Assert.ThrowsAsync<TaskCanceledException>(() => pipeline!(context));
        Assert.False(middlewareCompleted);
    }

    public class TestMiddleware : IGraphQLRequestMiddleware
    {
        public bool WasInvoked { get; private set; }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            WasInvoked = true;
            await next(context);
        }
    }

    public class TestMiddlewareWithArgs : IGraphQLRequestMiddleware
    {
        private readonly string _testValue;

        public TestMiddlewareWithArgs(string testValue)
        {
            _testValue = testValue;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            context.Features.Set(_testValue);
            await next(context);
        }
    }
}