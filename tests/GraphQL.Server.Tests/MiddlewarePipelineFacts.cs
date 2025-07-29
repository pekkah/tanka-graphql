using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Request;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.Response;

using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public class MiddlewarePipelineFacts
{
    [Fact]
    public async Task Middleware_SingleMiddleware_ShouldExecute()
    {
        var executionOrder = new List<string>();
        var middleware = new TestMiddleware("test", executionOrder);
        
        var context = new GraphQLRequestContext();
        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            executionOrder.Add("final");
            return Task.CompletedTask;
        };

        await middleware.Invoke(context, finalDelegate);

        Assert.Equal(new[] { "test-before", "final", "test-after" }, executionOrder);
    }

    [Fact]
    public async Task Middleware_MultipleMiddleware_ShouldExecuteInOrder()
    {
        var executionOrder = new List<string>();
        var middleware1 = new TestMiddleware("middleware1", executionOrder);
        var middleware2 = new TestMiddleware("middleware2", executionOrder);
        var middleware3 = new TestMiddleware("middleware3", executionOrder);

        var context = new GraphQLRequestContext();
        
        // Build pipeline manually
        GraphQLRequestDelegate pipeline = async ctx =>
        {
            GraphQLRequestDelegate delegate3 = ctx3 =>
            {
                executionOrder.Add("final");
                return Task.CompletedTask;
            };
            
            GraphQLRequestDelegate delegate2 = async ctx2 =>
            {
                await middleware3.Invoke(ctx2, delegate3);
            };
            
            GraphQLRequestDelegate delegate1 = async ctx1 =>
            {
                await middleware2.Invoke(ctx1, delegate2);
            };
            
            await middleware1.Invoke(ctx, delegate1);
        };

        await pipeline(context);

        Assert.Equal(new[] 
        { 
            "middleware1-before", 
            "middleware2-before", 
            "middleware3-before", 
            "final", 
            "middleware3-after", 
            "middleware2-after", 
            "middleware1-after" 
        }, executionOrder);
    }

    [Fact]
    public async Task Middleware_WithException_ShouldPropagateException()
    {
        var middleware = new ExceptionThrowingMiddleware();
        
        var context = new GraphQLRequestContext();
        GraphQLRequestDelegate finalDelegate = ctx => Task.CompletedTask;

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            middleware.Invoke(context, finalDelegate).AsTask());
    }

    [Fact]
    public async Task Middleware_WithExceptionHandling_ShouldCatchAndHandle()
    {
        var executionOrder = new List<string>();
        var exceptionMiddleware = new ExceptionThrowingMiddleware();
        var handlingMiddleware = new ExceptionHandlingMiddleware(executionOrder);

        var context = new GraphQLRequestContext();
        
        GraphQLRequestDelegate pipeline = async ctx =>
        {
            GraphQLRequestDelegate finalDelegate = ctx2 =>
            {
                executionOrder.Add("final");
                return Task.CompletedTask;
            };
            
            GraphQLRequestDelegate exceptionDelegate = async ctx1 =>
            {
                await exceptionMiddleware.Invoke(ctx1, finalDelegate);
            };
            
            await handlingMiddleware.Invoke(ctx, exceptionDelegate);
        };

        await pipeline(context);

        Assert.Equal(new[] { "handling-before", "exception-caught" }, executionOrder);
        // Verify that execution result was set in the response stream
        await foreach (var result in context.Response)
        {
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            break; // Only check first result
        }
    }

    [Fact]
    public async Task Middleware_ShortCircuit_ShouldSkipRemainingMiddleware()
    {
        var executionOrder = new List<string>();
        var middleware1 = new TestMiddleware("middleware1", executionOrder);
        var shortCircuitMiddleware = new ShortCircuitMiddleware(executionOrder);
        var middleware3 = new TestMiddleware("middleware3", executionOrder);

        var context = new GraphQLRequestContext();
        
        GraphQLRequestDelegate pipeline = async ctx =>
        {
            GraphQLRequestDelegate finalDelegate = ctx3 =>
            {
                executionOrder.Add("final");
                return Task.CompletedTask;
            };
            
            GraphQLRequestDelegate middleware3Delegate = async ctx2 =>
            {
                await middleware3.Invoke(ctx2, finalDelegate);
            };
            
            GraphQLRequestDelegate shortCircuitDelegate = async ctx1 =>
            {
                await shortCircuitMiddleware.Invoke(ctx1, middleware3Delegate);
            };
            
            await middleware1.Invoke(ctx, shortCircuitDelegate);
        };

        await pipeline(context);

        Assert.Equal(new[] 
        { 
            "middleware1-before", 
            "short-circuit", 
            "middleware1-after" 
        }, executionOrder);
    }

    [Fact]
    public async Task Middleware_ModifyContext_ShouldPropagateChanges()
    {
        var context = new GraphQLRequestContext();
        var modifyingMiddleware = new ContextModifyingMiddleware();
        
        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            Assert.NotNull(ctx.Request);
            Assert.Equal("{ modifiedQuery }", ctx.Request.Query.ToString());
            Assert.Equal("modified-operation", ctx.Request.OperationName);
            return Task.CompletedTask;
        };

        await modifyingMiddleware.Invoke(context, finalDelegate);
    }

    [Fact]
    public async Task Middleware_WithHttpContext_ShouldAccessHttpFeatures()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Test-Header"] = "test-value";
        
        var context = new GraphQLRequestContext();
        context.HttpContext = httpContext;

        var httpMiddleware = new HttpContextAccessingMiddleware();
        var headerValue = string.Empty;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            var testFeature = ctx.Features.Get<ITestFeature>();
            headerValue = testFeature?.TestHeader ?? string.Empty;
            return Task.CompletedTask;
        };

        await httpMiddleware.Invoke(context, finalDelegate);

        Assert.Equal("test-value", headerValue);
    }

    [Fact]
    public async Task Middleware_WithServiceProvider_ShouldResolveServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();

        var context = new GraphQLRequestContext();
        context.RequestServices = serviceProvider;

        var serviceMiddleware = new ServiceResolvingMiddleware();
        var serviceCalled = false;

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            var testFeature = ctx.Features.Get<ITestFeature>();
            serviceCalled = testFeature?.ServiceCalled ?? false;
            return Task.CompletedTask;
        };

        await serviceMiddleware.Invoke(context, finalDelegate);

        Assert.True(serviceCalled);
    }

    [Fact]
    public async Task Middleware_MeasurePerformance_ShouldRecordTiming()
    {
        var timings = new Dictionary<string, TimeSpan>();
        var performanceMiddleware = new PerformanceMiddleware(timings);
        
        var context = new GraphQLRequestContext();
        
        GraphQLRequestDelegate finalDelegate = async ctx =>
        {
            await Task.Delay(50); // Simulate work
        };

        await performanceMiddleware.Invoke(context, finalDelegate);

        Assert.True(timings.ContainsKey("GraphQL.Execution"));
        Assert.True(timings["GraphQL.Execution"].TotalMilliseconds >= 50);
    }

    [Fact]
    public async Task Middleware_WithConditionalExecution_ShouldExecuteBasedOnCondition()
    {
        var executionOrder = new List<string>();
        var conditionalMiddleware = new ConditionalMiddleware(
            ctx => ctx.Request?.OperationName == "allowed",
            executionOrder
        );

        // Test allowed operation
        var context1 = new GraphQLRequestContext();
        context1.Request = new GraphQLRequest { Query = "{ field }", OperationName = "allowed" };

        GraphQLRequestDelegate finalDelegate1 = ctx =>
        {
            executionOrder.Add("final");
            return Task.CompletedTask;
        };
        await conditionalMiddleware.Invoke(context1, finalDelegate1);

        Assert.Equal(new[] { "conditional-executed", "final" }, executionOrder);

        // Test blocked operation
        executionOrder.Clear();
        var context2 = new GraphQLRequestContext();
        context2.Request = new GraphQLRequest { Query = "{ field }", OperationName = "blocked" };

        GraphQLRequestDelegate finalDelegate2 = ctx =>
        {
            executionOrder.Add("final");
            return Task.CompletedTask;
        };
        await conditionalMiddleware.Invoke(context2, finalDelegate2);

        Assert.Equal(new[] { "conditional-skipped" }, executionOrder);
    }

    [Fact]
    public async Task Middleware_AsyncInitialization_ShouldCompleteBeforeNext()
    {
        var executionOrder = new List<string>();
        var asyncMiddleware = new AsyncInitializationMiddleware(executionOrder);
        
        var context = new GraphQLRequestContext();

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            executionOrder.Add("final");
            var testFeature = ctx.Features.Get<ITestFeature>();
            Assert.True(testFeature?.AsyncInitialized ?? false);
            return Task.CompletedTask;
        };
        await asyncMiddleware.Invoke(context, finalDelegate);

        Assert.Equal(new[] { "async-init-start", "async-init-complete", "final" }, executionOrder);
    }

    [Fact]
    public async Task Middleware_WithRetry_ShouldRetryOnFailure()
    {
        var attemptCount = 0;
        var retryMiddleware = new RetryMiddleware(maxRetries: 3);
        
        var context = new GraphQLRequestContext();

        GraphQLRequestDelegate finalDelegate = ctx =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new InvalidOperationException("Transient error");
            }
            return Task.CompletedTask;
        };
        await retryMiddleware.Invoke(context, finalDelegate);

        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public async Task Middleware_WithCircuitBreaker_ShouldOpenOnFailures()
    {
        var circuitBreakerMiddleware = new CircuitBreakerMiddleware(failureThreshold: 2);
        var context = new GraphQLRequestContext();
        var callCount = 0;

        // First two calls should fail and open the circuit
        for (int i = 0; i < 2; i++)
        {
            try
            {
                GraphQLRequestDelegate failingDelegate = ctx =>
                {
                    callCount++;
                    throw new InvalidOperationException("Service unavailable");
                };
                await circuitBreakerMiddleware.Invoke(context, failingDelegate);
            }
            catch { }
        }

        // Third call should be rejected immediately
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            GraphQLRequestDelegate successDelegate = ctx =>
            {
                callCount++;
                return Task.CompletedTask;
            };
            await circuitBreakerMiddleware.Invoke(context, successDelegate);
        });

        Assert.Equal(2, callCount); // Only first two calls went through
    }

    // Test middleware implementations
    private class TestMiddleware : IGraphQLRequestMiddleware
    {
        private readonly string _name;
        private readonly List<string> _executionOrder;

        public TestMiddleware(string name, List<string> executionOrder)
        {
            _name = name;
            _executionOrder = executionOrder;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            _executionOrder.Add($"{_name}-before");
            await next(context);
            _executionOrder.Add($"{_name}-after");
        }
    }

    private class ExceptionThrowingMiddleware : IGraphQLRequestMiddleware
    {
        public ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            throw new InvalidOperationException("Test exception");
        }
    }

    private class ExceptionHandlingMiddleware : IGraphQLRequestMiddleware
    {
        private readonly List<string> _executionOrder;

        public ExceptionHandlingMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            _executionOrder.Add("handling-before");
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _executionOrder.Add("exception-caught");
                // Set error result in response stream
                var errorResult = new ExecutionResult
                {
                    Errors = new[] { new ExecutionError { Message = ex.Message } }
                };
                context.Response = CreateAsyncEnumerable(errorResult);
            }
        }
    }

    private class ShortCircuitMiddleware : IGraphQLRequestMiddleware
    {
        private readonly List<string> _executionOrder;

        public ShortCircuitMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            _executionOrder.Add("short-circuit");
            // Don't call next - short circuit the pipeline
            return ValueTask.CompletedTask;
        }
    }

    private class ContextModifyingMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            context.Request = new GraphQLRequest
            {
                Query = "{ modifiedQuery }",
                OperationName = "modified-operation"
            };
            
            await next(context);
        }
    }

    private class HttpContextAccessingMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            if (context.HttpContext != null)
            {
                var headerValue = context.HttpContext.Request.Headers["X-Test-Header"].ToString();
                var testFeature = context.Features.Get<ITestFeature>() ?? new TestFeature();
                context.Features.Set<ITestFeature>(testFeature);
                testFeature.TestHeader = headerValue;
            }
            
            await next(context);
        }
    }

    private class ServiceResolvingMiddleware : IGraphQLRequestMiddleware
    {
        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var service = context.RequestServices?.GetService<ITestService>();
            if (service != null)
            {
                var testFeature = context.Features.Get<ITestFeature>() ?? new TestFeature();
                context.Features.Set<ITestFeature>(testFeature);
                testFeature.ServiceCalled = service.WasCalled();
            }
            
            await next(context);
        }
    }

    private class PerformanceMiddleware : IGraphQLRequestMiddleware
    {
        private readonly Dictionary<string, TimeSpan> _timings;

        public PerformanceMiddleware(Dictionary<string, TimeSpan> timings)
        {
            _timings = timings;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var start = DateTime.UtcNow;
            await next(context);
            var duration = DateTime.UtcNow - start;
            
            _timings["GraphQL.Execution"] = duration;
        }
    }

    private class ConditionalMiddleware : IGraphQLRequestMiddleware
    {
        private readonly Func<GraphQLRequestContext, bool> _condition;
        private readonly List<string> _executionOrder;

        public ConditionalMiddleware(Func<GraphQLRequestContext, bool> condition, List<string> executionOrder)
        {
            _condition = condition;
            _executionOrder = executionOrder;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            if (_condition(context))
            {
                _executionOrder.Add("conditional-executed");
                await next(context);
            }
            else
            {
                _executionOrder.Add("conditional-skipped");
            }
        }
    }

    private class AsyncInitializationMiddleware : IGraphQLRequestMiddleware
    {
        private readonly List<string> _executionOrder;

        public AsyncInitializationMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            _executionOrder.Add("async-init-start");
            await Task.Delay(10); // Simulate async initialization
            var testFeature = context.Features.Get<ITestFeature>() ?? new TestFeature();
            context.Features.Set<ITestFeature>(testFeature);
            testFeature.AsyncInitialized = true;
            _executionOrder.Add("async-init-complete");
            
            await next(context);
        }
    }

    private class RetryMiddleware : IGraphQLRequestMiddleware
    {
        private readonly int _maxRetries;

        public RetryMiddleware(int maxRetries)
        {
            _maxRetries = maxRetries;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            var retries = 0;
            while (retries < _maxRetries)
            {
                try
                {
                    await next(context);
                    return;
                }
                catch
                {
                    retries++;
                    if (retries >= _maxRetries)
                        throw;
                }
            }
        }
    }

    private class CircuitBreakerMiddleware : IGraphQLRequestMiddleware
    {
        private readonly int _failureThreshold;
        private int _failureCount;
        private bool _isOpen;

        public CircuitBreakerMiddleware(int failureThreshold)
        {
            _failureThreshold = failureThreshold;
        }

        public async ValueTask Invoke(GraphQLRequestContext context, GraphQLRequestDelegate next)
        {
            if (_isOpen)
            {
                throw new InvalidOperationException("Circuit breaker is open");
            }

            try
            {
                await next(context);
                _failureCount = 0; // Reset on success
            }
            catch
            {
                _failureCount++;
                if (_failureCount >= _failureThreshold)
                {
                    _isOpen = true;
                }
                throw;
            }
        }
    }

    // Test service interface
    private interface ITestService
    {
        bool WasCalled();
    }

    private class TestService : ITestService
    {
        public bool WasCalled() => true;
    }

    // Test feature for storing test data
    private interface ITestFeature
    {
        string? TestHeader { get; set; }
        bool ServiceCalled { get; set; }
        bool AsyncInitialized { get; set; }
    }

    private class TestFeature : ITestFeature
    {
        public string? TestHeader { get; set; }
        public bool ServiceCalled { get; set; }
        public bool AsyncInitialized { get; set; }
    }

    // Helper method to create async enumerable
    private static async IAsyncEnumerable<ExecutionResult> CreateAsyncEnumerable(ExecutionResult result)
    {
        yield return result;
        await Task.CompletedTask;
    }
}