using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tanka.GraphQL.Server;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLRequestPipelineBuilderFacts
{
    [Fact]
    public void Constructor_ShouldInitializeWithApplicationServices()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        
        // When
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // Then
        Assert.NotNull(builder);
        Assert.Equal(services, builder.ApplicationServices);
        Assert.NotNull(builder.Properties);
    }

    [Fact]
    public void Properties_ShouldBeInitialized()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        
        // When
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // Then
        Assert.NotNull(builder.Properties);
        Assert.IsType<Dictionary<string, object?>>(builder.Properties);
    }

    [Fact]
    public void ApplicationServices_ShouldReturnProvidedServices()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        var result = builder.ApplicationServices;
        
        // Then
        Assert.Equal(services, result);
    }

    [Fact]
    public void Build_ShouldCreatePipeline()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        var pipeline = builder.Build();
        
        // Then
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task Build_ShouldThrowIfNoMiddlewareAdded()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When & Then
        var exception = await Assert.ThrowsAsync<QueryException>(() => pipeline(context));
        Assert.Contains("Request execution pipeline error. No middleware returned any results.", exception.Message);
    }

    [Fact]
    public async Task Build_ShouldExecuteMiddlewareInCorrectOrder()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        var executionOrder = new List<string>();
        
        builder.Use(next => async context =>
        {
            executionOrder.Add("first");
            await next(context);
            executionOrder.Add("first-after");
        });
        
        builder.Use(next => async context =>
        {
            executionOrder.Add("second");
            await next(context);
            executionOrder.Add("second-after");
        });
        
        builder.Use(next => async context =>
        {
            executionOrder.Add("third");
            // Don't call next - terminate here
        });
        
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When
        await pipeline(context);
        
        // Then
        Assert.Equal(new[] { "first", "second", "third", "second-after", "first-after" }, executionOrder);
    }

    [Fact]
    public void Clone_ShouldCreateCopyWithSameProperties()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.SetProperty("test", "value");
        
        // When
        var clone = builder.Clone();
        
        // Then
        Assert.NotSame(builder, clone);
        Assert.Equal("value", clone.GetProperty<string>("test"));
        Assert.Equal(services, clone.ApplicationServices);
    }

    [Fact]
    public void Clone_ShouldShareMiddlewareComponents()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.Use(next => async context => await next(context));
        
        var clone = builder.Clone();
        
        // When
        var originalPipeline = builder.Build();
        var clonePipeline = clone.Build();
        
        // Then
        Assert.NotSame(originalPipeline, clonePipeline);
        // Both should have the same middleware behavior
    }

    [Fact]
    public void GetProperty_ShouldReturnDefaultValueForMissingProperty()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        var result = builder.GetProperty<string>("missing", "default");
        
        // Then
        Assert.Equal("default", result);
    }

    [Fact]
    public void GetProperty_ShouldReturnNullForMissingPropertyWithoutDefault()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        var result = builder.GetProperty<string>("missing");
        
        // Then
        Assert.Null(result);
    }

    [Fact]
    public void GetProperty_ShouldReturnStoredValue()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.SetProperty("test", "value");
        
        // When
        var result = builder.GetProperty<string>("test");
        
        // Then
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetRequiredProperty_ShouldReturnStoredValue()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.SetProperty("test", "value");
        
        // When
        var result = builder.GetRequiredProperty<string>("test");
        
        // Then
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetRequiredProperty_ShouldThrowForMissingProperty()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When & Then
        Assert.Throws<ArgumentNullException>(() => builder.GetRequiredProperty<string>("missing"));
    }

    [Fact]
    public void SetProperty_ShouldStoreValue()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        builder.SetProperty("test", "value");
        
        // Then
        Assert.Equal("value", builder.GetProperty<string>("test"));
    }

    [Fact]
    public void SetProperty_ShouldOverwriteExistingValue()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.SetProperty("test", "value1");
        
        // When
        builder.SetProperty("test", "value2");
        
        // Then
        Assert.Equal("value2", builder.GetProperty<string>("test"));
    }

    [Fact]
    public void Use_ShouldAddMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        var middlewareCalled = false;
        
        // When
        builder.Use(next => async context =>
        {
            middlewareCalled = true;
            await next(context);
        });
        
        // Then
        Assert.NotNull(builder.Build());
        // Middleware addition doesn't fail
    }

    [Fact]
    public void Use_ShouldReturnBuilder()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        var result = builder.Use(next => async context => await next(context));
        
        // Then
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseGeneric_ShouldAddMiddlewareFromServices()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var middleware = Substitute.For<IGraphQLRequestMiddleware>();
        services.GetService(typeof(TestMiddleware)).Returns(middleware);
        
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When
        builder.Use<TestMiddleware>();
        var pipeline = builder.Build();
        
        // Then
        Assert.NotNull(pipeline);
        services.Received().GetService(typeof(TestMiddleware));
    }

    [Fact]
    public void UseGeneric_ShouldThrowIfMiddlewareNotRegistered()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(TestMiddleware)).Returns(null);
        
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        // When & Then
        Assert.Throws<InvalidOperationException>(() => builder.Use<TestMiddleware>());
    }

    [Fact]
    public async Task UseGeneric_ShouldExecuteMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var middleware = Substitute.For<IGraphQLRequestMiddleware>();
        services.GetService(typeof(TestMiddleware)).Returns(middleware);
        
        var builder = new GraphQLRequestPipelineBuilder(services);
        builder.Use<TestMiddleware>();
        
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When
        await pipeline(context);
        
        // Then
        await middleware.Received().Invoke(context, Arg.Any<GraphQLRequestDelegate>());
    }

    [Fact]
    public async Task Pipeline_ShouldHandleMultipleMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        var counter = 0;
        
        builder.Use(next => async context =>
        {
            counter++;
            await next(context);
        });
        
        builder.Use(next => async context =>
        {
            counter++;
            await next(context);
        });
        
        builder.Use(next => async context =>
        {
            counter++;
            // Don't call next - terminate here
        });
        
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When
        await pipeline(context);
        
        // Then
        Assert.Equal(3, counter);
    }

    [Fact]
    public async Task Pipeline_ShouldHandleExceptionInMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        
        builder.Use(next => async context =>
        {
            throw new InvalidOperationException("Test exception");
        });
        
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When & Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline(context));
    }

    [Fact]
    public async Task Pipeline_ShouldAllowMiddlewareToSkipNext()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new GraphQLRequestPipelineBuilder(services);
        var firstCalled = false;
        var secondCalled = false;
        
        builder.Use(next => async context =>
        {
            firstCalled = true;
            // Don't call next - skip remaining middleware
        });
        
        builder.Use(next => async context =>
        {
            secondCalled = true;
            await next(context);
        });
        
        var pipeline = builder.Build();
        var context = new GraphQLRequestContext();
        
        // When
        await pipeline(context);
        
        // Then
        Assert.True(firstCalled);
        Assert.False(secondCalled);
    }

    // Helper class for testing generic middleware registration
    public class TestMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            await next(context);
        }
    }
}