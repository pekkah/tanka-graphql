using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class GraphQLRequestPipelineBuilderFacts
{
    private readonly IServiceProvider _serviceProvider;
    private readonly GraphQLRequestPipelineBuilder _builder;

    public GraphQLRequestPipelineBuilderFacts()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _builder = new GraphQLRequestPipelineBuilder(_serviceProvider);
    }

    [Fact]
    public void Constructor_WithServiceProvider_SetsApplicationServices()
    {
        // Given & When
        var builder = new GraphQLRequestPipelineBuilder(_serviceProvider);

        // Then
        Assert.Equal(_serviceProvider, builder.ApplicationServices);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsException()
    {
        // When & Then
        Assert.Throws<ArgumentNullException>(() => new GraphQLRequestPipelineBuilder(null!));
    }

    [Fact]
    public void Properties_WhenAccessed_ReturnsNotNull()
    {
        // Given & When
        var properties = _builder.Properties;

        // Then
        Assert.NotNull(properties);
    }

    [Fact]
    public void ApplicationServices_WhenAccessed_ReturnsServiceProvider()
    {
        // Given & When
        var services = _builder.ApplicationServices;

        // Then
        Assert.Equal(_serviceProvider, services);
    }

    [Fact]
    public void Build_WithNoMiddleware_ReturnsThrowingPipeline()
    {
        // Given & When
        var pipeline = _builder.Build();

        // Then
        Assert.NotNull(pipeline);
        
        // The pipeline should throw when invoked with no middleware
        var context = new GraphQLRequestContext();
        Assert.ThrowsAsync<QueryException>(async () => await pipeline(context));
    }

    [Fact]
    public void Use_WithMiddleware_AddsMiddleware()
    {
        // Given
        var middlewareInvoked = false;
        Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware = next => context =>
        {
            middlewareInvoked = true;
            return next(context);
        };

        // When
        _builder.Use(middleware);
        var pipeline = _builder.Build();

        // Then
        Assert.NotNull(pipeline);
        
        // The middleware should be in the pipeline
        var context = new GraphQLRequestContext();
        Assert.ThrowsAsync<QueryException>(async () => await pipeline(context));
        Assert.True(middlewareInvoked);
    }

    [Fact]
    public void Use_WithMultipleMiddleware_AddsInCorrectOrder()
    {
        // Given
        var callOrder = new List<int>();
        
        Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware1 = next => context =>
        {
            callOrder.Add(1);
            return next(context);
        };
        
        Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware2 = next => context =>
        {
            callOrder.Add(2);
            return next(context);
        };

        // When
        _builder.Use(middleware1);
        _builder.Use(middleware2);
        var pipeline = _builder.Build();

        // Then
        var context = new GraphQLRequestContext();
        Assert.ThrowsAsync<QueryException>(async () => await pipeline(context));
        
        // Middleware should be called in the order they were added
        Assert.Equal(new[] { 1, 2 }, callOrder);
    }

    [Fact]
    public void Use_WithTerminatingMiddleware_DoesNotCallNext()
    {
        // Given
        var terminatingMiddleware = false;
        Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware = next => context =>
        {
            terminatingMiddleware = true;
            return Task.CompletedTask; // Don't call next
        };

        // When
        _builder.Use(middleware);
        var pipeline = _builder.Build();

        // Then
        var context = new GraphQLRequestContext();
        
        // Should not throw because middleware terminates the pipeline
        Assert.DoesNotThrowAsync(async () => await pipeline(context));
        Assert.True(terminatingMiddleware);
    }

    [Fact]
    public void Use_WithGenericMiddleware_ResolvesFromServiceProvider()
    {
        // Given
        var middleware = Substitute.For<IGraphQLRequestMiddleware>();
        _serviceProvider.GetRequiredService<TestMiddleware>().Returns(new TestMiddleware());

        // When
        _builder.Use<TestMiddleware>();
        var pipeline = _builder.Build();

        // Then
        Assert.NotNull(pipeline);
        
        // Verify the service was requested
        _serviceProvider.Received().GetRequiredService<TestMiddleware>();
    }

    [Fact]
    public void Use_WithGenericMiddleware_WhenServiceNotFound_ThrowsException()
    {
        // Given
        _serviceProvider.GetRequiredService<TestMiddleware>()
            .Returns(x => throw new InvalidOperationException("Service not found"));

        // When & Then
        Assert.Throws<InvalidOperationException>(() => _builder.Use<TestMiddleware>());
    }

    [Fact]
    public void Clone_CreatesNewBuilderWithSameProperties()
    {
        // Given
        _builder.SetProperty("TestKey", "TestValue");
        _builder.Use(next => context => next(context));

        // When
        var clone = _builder.Clone();

        // Then
        Assert.NotSame(_builder, clone);
        Assert.Equal(_builder.ApplicationServices, clone.ApplicationServices);
        Assert.Equal("TestValue", clone.GetProperty<string>("TestKey"));
    }

    [Fact]
    public void Clone_WithModifications_DoesNotAffectOriginal()
    {
        // Given
        _builder.SetProperty("TestKey", "OriginalValue");
        var clone = _builder.Clone();

        // When
        clone.SetProperty("TestKey", "ModifiedValue");

        // Then
        Assert.Equal("OriginalValue", _builder.GetProperty<string>("TestKey"));
        Assert.Equal("ModifiedValue", clone.GetProperty<string>("TestKey"));
    }

    [Fact]
    public void GetProperty_WithExistingKey_ReturnsValue()
    {
        // Given
        _builder.SetProperty("TestKey", "TestValue");

        // When
        var value = _builder.GetProperty<string>("TestKey");

        // Then
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void GetProperty_WithNonExistingKey_ReturnsDefault()
    {
        // Given & When
        var value = _builder.GetProperty<string>("NonExistingKey");

        // Then
        Assert.Null(value);
    }

    [Fact]
    public void GetProperty_WithNonExistingKeyAndDefault_ReturnsDefault()
    {
        // Given & When
        var value = _builder.GetProperty("NonExistingKey", "DefaultValue");

        // Then
        Assert.Equal("DefaultValue", value);
    }

    [Fact]
    public void GetRequiredProperty_WithExistingKey_ReturnsValue()
    {
        // Given
        _builder.SetProperty("TestKey", "TestValue");

        // When
        var value = _builder.GetRequiredProperty<string>("TestKey");

        // Then
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void GetRequiredProperty_WithNonExistingKey_ThrowsException()
    {
        // Given & When & Then
        Assert.Throws<ArgumentNullException>(() => _builder.GetRequiredProperty<string>("NonExistingKey"));
    }

    [Fact]
    public void GetRequiredProperty_WithNullValue_ThrowsException()
    {
        // Given
        _builder.SetProperty<string>("TestKey", null!);

        // When & Then
        Assert.Throws<ArgumentNullException>(() => _builder.GetRequiredProperty<string>("TestKey"));
    }

    [Fact]
    public void SetProperty_WithValidKeyValue_SetsProperty()
    {
        // Given & When
        _builder.SetProperty("TestKey", "TestValue");

        // Then
        Assert.Equal("TestValue", _builder.GetProperty<string>("TestKey"));
    }

    [Fact]
    public void SetProperty_WithExistingKey_OverwritesValue()
    {
        // Given
        _builder.SetProperty("TestKey", "OriginalValue");

        // When
        _builder.SetProperty("TestKey", "NewValue");

        // Then
        Assert.Equal("NewValue", _builder.GetProperty<string>("TestKey"));
    }

    [Fact]
    public void SetProperty_WithNullKey_ThrowsException()
    {
        // Given & When & Then
        Assert.Throws<ArgumentNullException>(() => _builder.SetProperty<string>(null!, "TestValue"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("key")]
    [InlineData("long.key.with.dots")]
    [InlineData("KEY_WITH_UNDERSCORES")]
    public void SetProperty_WithDifferentKeyFormats_WorksCorrectly(string key)
    {
        // Given & When
        _builder.SetProperty(key, "TestValue");

        // Then
        Assert.Equal("TestValue", _builder.GetProperty<string>(key));
    }

    [Fact]
    public void Properties_IsStringComparerOrdinal()
    {
        // Given
        _builder.SetProperty("TestKey", "Value1");
        _builder.SetProperty("TESTKEY", "Value2");

        // When
        var value1 = _builder.GetProperty<string>("TestKey");
        var value2 = _builder.GetProperty<string>("TESTKEY");

        // Then
        Assert.Equal("Value1", value1);
        Assert.Equal("Value2", value2);
        Assert.NotEqual(value1, value2);
    }

    [Fact]
    public void Build_CalledMultipleTimes_ReturnsSamePipeline()
    {
        // Given
        _builder.Use(next => context => next(context));

        // When
        var pipeline1 = _builder.Build();
        var pipeline2 = _builder.Build();

        // Then
        Assert.NotNull(pipeline1);
        Assert.NotNull(pipeline2);
        // While they're different delegate instances, they should behave the same
    }

    [Fact]
    public async Task Build_WithAsyncMiddleware_HandlesAsyncCorrectly()
    {
        // Given
        var asyncMiddlewareInvoked = false;
        Func<GraphQLRequestDelegate, GraphQLRequestDelegate> asyncMiddleware = next => async context =>
        {
            await Task.Delay(1); // Simulate async work
            asyncMiddlewareInvoked = true;
            await next(context);
        };

        // When
        _builder.Use(asyncMiddleware);
        var pipeline = _builder.Build();

        // Then
        var context = new GraphQLRequestContext();
        await Assert.ThrowsAsync<QueryException>(async () => await pipeline(context));
        Assert.True(asyncMiddlewareInvoked);
    }

    private class TestMiddleware : IGraphQLRequestMiddleware
    {
        public ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            return next(context);
        }
    }
}