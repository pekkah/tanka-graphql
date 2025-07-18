using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

public class MiddlewarePipelineFacts
{
    private readonly ISchema _schema;
    private readonly ResolversMap _resolvers;
    private readonly IServiceProvider _serviceProvider;

    public MiddlewarePipelineFacts()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        _serviceProvider = services.BuildServiceProvider();

        var sdl = @"
            type Query {
                hello: String
                error: String
                slow: String
            }
        ";

        _resolvers = new ResolversMap
        {
            {
                "Query", new FieldResolversMap
                {
                    { "hello", context => context.ResolveAs("Hello World!") },
                    { "error", context => throw new InvalidOperationException("Test error") },
                    { "slow", async context => 
                        {
                            await Task.Delay(100, context.CancellationToken);
                            return context.ResolveAs("Slow result");
                        }
                    }
                }
            }
        };

        _schema = new SchemaBuilder()
            .Add(sdl)
            .Build(_resolvers, _resolvers).Result;
    }

    [Fact]
    public async Task OperationDelegateBuilder_BuildsCorrectly()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);

        /* When */
        builder.Use(async (context, next) => await next(context));
        var pipeline = builder.Build();

        /* Then */
        Assert.NotNull(pipeline);
    }

    [Fact]
    public async Task OperationDelegateBuilder_SingleMiddleware_ExecutesCorrectly()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var middlewareExecuted = false;

        builder.Use(async (context, next) =>
        {
            middlewareExecuted = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.True(middlewareExecuted);
    }

    [Fact]
    public async Task OperationDelegateBuilder_MultipleMiddleware_ExecutesInOrder()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var executionOrder = new List<string>();

        builder.Use(async (context, next) =>
        {
            executionOrder.Add("First");
            await next(context);
        });

        builder.Use(async (context, next) =>
        {
            executionOrder.Add("Second");
            await next(context);
        });

        builder.Use(async (context, next) =>
        {
            executionOrder.Add("Third");
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.Equal(new[] { "First", "Second", "Third" }, executionOrder);
    }

    [Fact]
    public async Task OperationDelegateBuilder_MiddlewareThrowsException_PropagatesException()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var expectedException = new InvalidOperationException("Test exception");

        builder.Use(async (context, next) =>
        {
            throw expectedException;
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline!(queryContext));

        /* Then */
        Assert.Equal(expectedException, exception);
    }

    [Fact]
    public async Task OperationDelegateBuilder_MiddlewareModifiesContext_ChangesAreVisible()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var testValue = "modified";

        builder.Use(async (context, next) =>
        {
            context.Features.Set(testValue);
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.Equal(testValue, queryContext.Features.Get<string>());
    }

    [Fact]
    public async Task OperationDelegateBuilder_WithCancellation_RespondsCorrectly()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var cancellationTokenSource = new CancellationTokenSource();
        var middlewareCompleted = false;

        builder.Use(async (context, next) =>
        {
            await Task.Delay(100, context.CancellationToken);
            middlewareCompleted = true;
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }", cancellationTokenSource.Token);

        /* When */
        cancellationTokenSource.Cancel();

        /* Then */
        await Assert.ThrowsAsync<TaskCanceledException>(() => pipeline!(queryContext));
        Assert.False(middlewareCompleted);
    }

    [Fact]
    public async Task OperationDelegateBuilder_ErrorHandlingMiddleware_CatchesErrors()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var errorCaught = false;

        builder.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception)
            {
                errorCaught = true;
                throw;
            }
        });

        builder.Use(async (context, next) =>
        {
            throw new InvalidOperationException("Test error");
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline!(queryContext));

        /* Then */
        Assert.True(errorCaught);
    }

    [Fact]
    public async Task OperationDelegateBuilder_TimingMiddleware_MeasuresExecutionTime()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var elapsedTime = TimeSpan.Zero;

        builder.Use(async (context, next) =>
        {
            var startTime = DateTime.UtcNow;
            await next(context);
            elapsedTime = DateTime.UtcNow - startTime;
        });

        builder.Use(async (context, next) =>
        {
            await Task.Delay(50);
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.True(elapsedTime.TotalMilliseconds >= 50);
    }

    [Fact]
    public async Task OperationDelegateBuilder_LoggingMiddleware_LogsExecution()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var logs = new List<string>();

        builder.Use(async (context, next) =>
        {
            logs.Add("Before execution");
            await next(context);
            logs.Add("After execution");
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.Equal(new[] { "Before execution", "After execution" }, logs);
    }

    [Fact]
    public async Task OperationDelegateBuilder_ConditionalMiddleware_ExecutesConditionally()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var middlewareExecuted = false;
        var shouldExecute = true;

        builder.Use(async (context, next) =>
        {
            if (shouldExecute)
            {
                middlewareExecuted = true;
            }
            await next(context);
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When - First execution */
        await pipeline!(queryContext);

        /* Then - First execution */
        Assert.True(middlewareExecuted);

        /* When - Second execution */
        middlewareExecuted = false;
        shouldExecute = false;
        await pipeline!(queryContext);

        /* Then - Second execution */
        Assert.False(middlewareExecuted);
    }

    [Fact]
    public async Task OperationDelegateBuilder_NestedMiddleware_ExecutesCorrectly()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var executionOrder = new List<string>();

        builder.Use(async (context, next) =>
        {
            executionOrder.Add("Outer-Start");
            await next(context);
            executionOrder.Add("Outer-End");
        });

        builder.Use(async (context, next) =>
        {
            executionOrder.Add("Inner-Start");
            await next(context);
            executionOrder.Add("Inner-End");
        });

        var pipeline = builder.Build();
        var queryContext = CreateQueryContext("query { hello }");

        /* When */
        await pipeline!(queryContext);

        /* Then */
        Assert.Equal(new[] { "Outer-Start", "Inner-Start", "Inner-End", "Outer-End" }, executionOrder);
    }

    [Fact]
    public async Task OperationDelegateBuilder_EmptyPipeline_DoesNothing()
    {
        /* Given */
        var builder = new OperationDelegateBuilder(_serviceProvider);
        var pipeline = builder.Build();

        /* When */
        if (pipeline != null)
        {
            var queryContext = CreateQueryContext("query { hello }");
            await pipeline(queryContext);
        }

        /* Then */
        // Should not throw an exception
        Assert.True(true);
    }

    private QueryContext CreateQueryContext(string query, CancellationToken cancellationToken = default)
    {
        var request = new GraphQLRequest
        {
            Query = query
        };

        return new QueryContext
        {
            Schema = _schema,
            Request = request,
            CancellationToken = cancellationToken,
            Features = new Features.FeatureCollection()
        };
    }
}