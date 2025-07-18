using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using Tanka.GraphQL.Validation;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

/// <summary>
/// Comprehensive tests for OperationDelegateBuilder middleware pipeline
/// focusing on middleware composition, order, and error handling
/// </summary>
public class OperationDelegateBuilderFacts
{
    private readonly IServiceProvider _serviceProvider;

    public OperationDelegateBuilderFacts()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncValidator, NoValidationFeature>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Build_WithoutMiddleware_ShouldThrowQueryException()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);

        // When
        var pipeline = builder.Build();

        // Then
        var context = CreateQueryContext();
        var exception = Assert.ThrowsAsync<QueryException>(async () => await pipeline(context));
        Assert.Equal("Operation execution pipeline error. No ending middleware.", exception.Result.Message);
    }

    [Fact]
    public async Task Build_WithSingleMiddleware_ShouldExecuteCorrectly()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var middlewareExecuted = false;

        builder.Use(next => async context =>
        {
            middlewareExecuted = true;
            await next(context);
        });

        builder.Use(_ => async context =>
        {
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "value" } };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();
        await pipeline(context);

        // Then
        Assert.True(middlewareExecuted);
        Assert.NotNull(context.Response);
    }

    [Fact]
    public async Task Build_WithMultipleMiddleware_ShouldExecuteInCorrectOrder()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var executionOrder = new List<string>();

        builder.Use(next => async context =>
        {
            executionOrder.Add("First");
            await next(context);
            executionOrder.Add("First_After");
        });

        builder.Use(next => async context =>
        {
            executionOrder.Add("Second");
            await next(context);
            executionOrder.Add("Second_After");
        });

        builder.Use(_ => async context =>
        {
            executionOrder.Add("Final");
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "value" } };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();
        await pipeline(context);

        // Then
        Assert.Equal(new[] { "First", "Second", "Final", "Second_After", "First_After" }, executionOrder);
    }

    [Fact]
    public async Task Build_WithMiddlewareThrowingException_ShouldPropagateException()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var exceptionMessage = "Test middleware exception";

        builder.Use(next => async context =>
        {
            await next(context);
        });

        builder.Use(_ => async context =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException(exceptionMessage);
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();

        // Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline(context));
        Assert.Equal(exceptionMessage, exception.Message);
    }

    [Fact]
    public async Task Build_WithMiddlewareNotCallingNext_ShouldNotExecuteSubsequentMiddleware()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var secondMiddlewareExecuted = false;

        builder.Use(next => async context =>
        {
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "terminated" } };
            // Intentionally not calling next
        });

        builder.Use(_ => async context =>
        {
            secondMiddlewareExecuted = true;
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();
        await pipeline(context);

        // Then
        Assert.False(secondMiddlewareExecuted);
        Assert.NotNull(context.Response);
    }

    [Fact]
    public async Task Build_WithConditionalMiddleware_ShouldExecuteBasedOnCondition()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var conditionalMiddlewareExecuted = false;

        builder.Use(next => async context =>
        {
            if (context.Request.OperationName == "TestOperation")
            {
                conditionalMiddlewareExecuted = true;
            }
            await next(context);
        });

        builder.Use(_ => async context =>
        {
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "value" } };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext("TestOperation");
        await pipeline(context);

        // Then
        Assert.True(conditionalMiddlewareExecuted);
    }

    [Fact]
    public async Task Build_WithAsyncMiddleware_ShouldHandleAsyncOperationsCorrectly()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var asyncTaskCompleted = false;

        builder.Use(next => async context =>
        {
            await Task.Delay(10);
            asyncTaskCompleted = true;
            await next(context);
        });

        builder.Use(_ => async context =>
        {
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "value" } };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();
        await pipeline(context);

        // Then
        Assert.True(asyncTaskCompleted);
    }

    [Fact]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Given
        var original = new OperationDelegateBuilder(_serviceProvider);
        var originalMiddlewareExecuted = false;
        var clonedMiddlewareExecuted = false;

        original.Use(next => async context =>
        {
            originalMiddlewareExecuted = true;
            await next(context);
        });

        // When
        var cloned = original.Clone();
        cloned.Use(next => async context =>
        {
            clonedMiddlewareExecuted = true;
            await next(context);
        });

        // Then
        Assert.NotSame(original, cloned);
        Assert.NotSame(original.Properties, cloned.Properties);
    }

    [Fact]
    public void Properties_ShouldSupportGetAndSetOperations()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var testValue = "test value";

        // When
        builder.SetProperty("testKey", testValue);
        var retrievedValue = builder.GetProperty<string>("testKey");

        // Then
        Assert.Equal(testValue, retrievedValue);
    }

    [Fact]
    public void GetRequiredProperty_WithMissingProperty_ShouldThrowException()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);

        // When & Then
        Assert.Throws<ArgumentNullException>(() => builder.GetRequiredProperty<string>("nonExistentKey"));
    }

    [Fact]
    public void GetProperty_WithMissingProperty_ShouldReturnDefault()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);

        // When
        var result = builder.GetProperty<string>("nonExistentKey", "defaultValue");

        // Then
        Assert.Equal("defaultValue", result);
    }

    [Fact]
    public async Task UseDefaults_ShouldSetupCompleteMiddlewarePipeline()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var schema = CreateTestSchema();
        var request = new GraphQLRequest
        {
            Query = "{ test }"
        };

        // When
        builder.UseDefaults();
        var pipeline = builder.Build();
        var context = new QueryContext(schema, request);

        // Then
        // Should not throw exception when executing with defaults
        await pipeline(context);
        Assert.NotNull(context.Response);
    }

    [Fact]
    public async Task Middleware_WithComplexErrorHandling_ShouldHandleMultipleExceptionTypes()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var errorMessages = new List<string>();

        builder.Use(next => async context =>
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                errorMessages.Add($"Validation: {ex.Message}");
                throw;
            }
            catch (FieldException ex)
            {
                errorMessages.Add($"Field: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                errorMessages.Add($"General: {ex.Message}");
                throw;
            }
        });

        builder.Use(_ => async context =>
        {
            throw new FieldException("Test field error")
            {
                Path = new NodePath()
            };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();

        // Then
        await Assert.ThrowsAsync<FieldException>(() => pipeline(context));
        Assert.Contains("Field: Test field error", errorMessages);
    }

    [Fact]
    public async Task Middleware_WithFeatureModification_ShouldModifyContextFeatures()
    {
        // Given
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var customFeature = Substitute.For<IFeature>();

        builder.Use(next => async context =>
        {
            context.Features.Set(customFeature);
            await next(context);
        });

        builder.Use(_ => async context =>
        {
            context.Response = new ExecutionResult { Data = new Dictionary<string, object?> { ["test"] = "value" } };
        });

        // When
        var pipeline = builder.Build();
        var context = CreateQueryContext();
        await pipeline(context);

        // Then
        Assert.Same(customFeature, context.Features.Get<IFeature>());
    }

    private static QueryContext CreateQueryContext(string? operationName = null)
    {
        var schema = CreateTestSchema();
        var request = new GraphQLRequest
        {
            Query = "{ test }",
            OperationName = operationName
        };

        return new QueryContext(schema, request);
    }

    private static ISchema CreateTestSchema()
    {
        var schemaBuilder = new SchemaBuilder();
        schemaBuilder.Add(@"
            type Query {
                test: String
            }
            schema {
                query: Query
            }
        ");

        var resolvers = new ResolversMap
        {
            ["Query"] = new FieldResolversMap
            {
                ["test"] = context => context.ResolveAs("test value")
            }
        };

        return schemaBuilder.Build(resolvers).Result;
    }
}

public interface IFeature
{
    // Marker interface for testing
}