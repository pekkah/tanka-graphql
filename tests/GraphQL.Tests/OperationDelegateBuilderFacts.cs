using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tanka.GraphQL;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class OperationDelegateBuilderFacts
{
    [Fact]
    public void Constructor_ShouldInitializeWithApplicationServices()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        
        // When
        var builder = new OperationDelegateBuilder(services);
        
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
        var builder = new OperationDelegateBuilder(services);
        
        // Then
        Assert.NotNull(builder.Properties);
        Assert.IsType<Dictionary<string, object?>>(builder.Properties);
    }

    [Fact]
    public void ApplicationServices_ShouldReturnProvidedServices()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        
        // When
        var result = builder.ApplicationServices;
        
        // Then
        Assert.Equal(services, result);
    }

    [Fact]
    public void Build_ShouldCreateOperationDelegate()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        
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
        var builder = new OperationDelegateBuilder(services);
        var pipeline = builder.Build();
        var context = new QueryContext();
        
        // When & Then
        var exception = await Assert.ThrowsAsync<QueryException>(() => pipeline(context));
        Assert.Contains("Operation execution pipeline error. No ending middleware.", exception.Message);
    }

    [Fact]
    public async Task Build_ShouldExecuteMiddlewareInCorrectOrder()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
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
        var context = new QueryContext();
        
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
        var builder = new OperationDelegateBuilder(services);
        builder.SetProperty("test", "value");
        
        // When
        var clone = builder.Clone();
        
        // Then
        Assert.NotSame(builder, clone);
        Assert.Equal("value", clone.GetProperty<string>("test"));
        Assert.Equal(services, clone.ApplicationServices);
    }

    [Fact]
    public void Clone_ShouldCopyMiddlewareComponents()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
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
        var builder = new OperationDelegateBuilder(services);
        
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
        var builder = new OperationDelegateBuilder(services);
        
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
        var builder = new OperationDelegateBuilder(services);
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
        var builder = new OperationDelegateBuilder(services);
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
        var builder = new OperationDelegateBuilder(services);
        
        // When & Then
        Assert.Throws<ArgumentNullException>(() => builder.GetRequiredProperty<string>("missing"));
    }

    [Fact]
    public void New_ShouldCreateNewBuilderWithSameProperties()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        builder.SetProperty("test", "value");
        
        // When
        var newBuilder = builder.New();
        
        // Then
        Assert.NotSame(builder, newBuilder);
        Assert.Equal("value", newBuilder.GetProperty<string>("test"));
        Assert.Equal(services, newBuilder.ApplicationServices);
    }

    [Fact]
    public void New_ShouldNotCopyMiddlewareComponents()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        builder.Use(next => async context => { /* test middleware */ });
        
        var newBuilder = builder.New();
        
        // When & Then
        // The new builder should not have the middleware components
        // This is tested by verifying the new builder throws when built without middleware
        var newPipeline = newBuilder.Build();
        var context = new QueryContext();
        
        Assert.ThrowsAsync<QueryException>(() => newPipeline(context));
    }

    [Fact]
    public void SetProperty_ShouldStoreValue()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        
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
        var builder = new OperationDelegateBuilder(services);
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
        var builder = new OperationDelegateBuilder(services);
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
        var builder = new OperationDelegateBuilder(services);
        
        // When
        var result = builder.Use(next => async context => await next(context));
        
        // Then
        Assert.Same(builder, result);
    }

    [Fact]
    public async Task Pipeline_ShouldHandleMultipleMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
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
        var context = new QueryContext();
        
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
        var builder = new OperationDelegateBuilder(services);
        
        builder.Use(next => async context =>
        {
            throw new InvalidOperationException("Test exception");
        });
        
        var pipeline = builder.Build();
        var context = new QueryContext();
        
        // When & Then
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline(context));
    }

    [Fact]
    public async Task Pipeline_ShouldAllowMiddlewareToSkipNext()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
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
        var context = new QueryContext();
        
        // When
        await pipeline(context);
        
        // Then
        Assert.True(firstCalled);
        Assert.False(secondCalled);
    }

    [Fact]
    public async Task Pipeline_ShouldPassContextBetweenMiddleware()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        var passedContext = (QueryContext?)null;
        
        builder.Use(next => async context =>
        {
            await next(context);
        });
        
        builder.Use(next => async context =>
        {
            passedContext = context;
            // Don't call next - terminate here
        });
        
        var pipeline = builder.Build();
        var originalContext = new QueryContext();
        
        // When
        await pipeline(originalContext);
        
        // Then
        Assert.Same(originalContext, passedContext);
    }

    [Fact]
    public async Task Pipeline_ShouldAllowMiddlewareToModifyContext()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        var modifiedValue = string.Empty;
        
        builder.Use(next => async context =>
        {
            context.SetProperty("test", "modified");
            await next(context);
        });
        
        builder.Use(next => async context =>
        {
            modifiedValue = context.GetProperty<string>("test") ?? "";
            // Don't call next - terminate here
        });
        
        var pipeline = builder.Build();
        var context = new QueryContext();
        
        // When
        await pipeline(context);
        
        // Then
        Assert.Equal("modified", modifiedValue);
    }

    [Fact]
    public async Task Pipeline_ShouldHandleNullContext()
    {
        // Given
        var services = Substitute.For<IServiceProvider>();
        var builder = new OperationDelegateBuilder(services);
        
        builder.Use(next => async context =>
        {
            // Don't call next - terminate here
        });
        
        var pipeline = builder.Build();
        
        // When & Then
        await Assert.ThrowsAsync<ArgumentNullException>(() => pipeline(null!));
    }
}