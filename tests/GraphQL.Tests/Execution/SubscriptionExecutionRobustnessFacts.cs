using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.ValueResolution;
using Xunit;

namespace Tanka.GraphQL.Tests.Execution;

/// <summary>
/// Comprehensive tests for subscription execution robustness,
/// focusing on error handling, cancellation token propagation, and cleanup
/// </summary>
public class SubscriptionExecutionRobustnessFacts
{
    [Fact]
    public async Task Subscribe_WithCancellationToken_ShouldRespectCancellation()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    longRunningCount
                }"
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // When
        var subscription = executor.Subscribe(request, cts.Token);

        // Then
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var result in subscription)
            {
                // Should be cancelled before completion
            }
        });
    }

    [Fact]
    public async Task Subscribe_WithStreamException_ShouldPropagateException()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    errorStream
                }"
        };

        // When
        var subscription = executor.Subscribe(request, CancellationToken.None);

        // Then
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var result in subscription)
            {
                // Should throw exception
            }
        });
    }

    [Fact]
    public async Task Subscribe_WithStreamExceptionInMiddle_ShouldHandleGracefully()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    partialErrorStream
                }"
        };

        var results = new List<ExecutionResult>();
        var exceptionThrown = false;

        // When
        try
        {
            await foreach (var result in executor.Subscribe(request, CancellationToken.None))
            {
                results.Add(result);
            }
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Then
        Assert.True(exceptionThrown);
        Assert.True(results.Count > 0); // Should have processed some results before exception
    }

    [Fact]
    public async Task Subscribe_WithResolverException_ShouldReturnErrorInResult()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    resolverErrorStream
                }"
        };

        // When
        var subscription = executor.Subscribe(request, CancellationToken.None);
        var results = new List<ExecutionResult>();

        await foreach (var result in subscription.Take(3))
        {
            results.Add(result);
        }

        // Then
        Assert.All(results, result => Assert.NotNull(result.Errors));
        Assert.All(results, result => Assert.NotEmpty(result.Errors));
        Assert.All(results, result => Assert.Contains("Resolver error", result.Errors.First().Message));
    }

    [Fact]
    public async Task Subscribe_WithMultipleSubscribers_ShouldHandleIndependently()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    count
                }"
        };

        using var cts1 = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        using var cts2 = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));

        // When
        var subscription1 = executor.Subscribe(request, cts1.Token);
        var subscription2 = executor.Subscribe(request, cts2.Token);

        var results1 = new List<ExecutionResult>();
        var results2 = new List<ExecutionResult>();

        var task1 = Task.Run(async () =>
        {
            try
            {
                await foreach (var result in subscription1)
                {
                    results1.Add(result);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        });

        var task2 = Task.Run(async () =>
        {
            try
            {
                await foreach (var result in subscription2)
                {
                    results2.Add(result);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        });

        await Task.WhenAll(task1, task2);

        // Then
        Assert.True(results1.Count > 0);
        Assert.True(results2.Count > 0);
        Assert.True(results2.Count >= results1.Count); // subscription2 ran longer
    }

    [Fact]
    public async Task Subscribe_WithEmptyStream_ShouldComplete()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    emptyStream
                }"
        };

        // When
        var subscription = executor.Subscribe(request, CancellationToken.None);
        var results = new List<ExecutionResult>();

        await foreach (var result in subscription)
        {
            results.Add(result);
        }

        // Then
        Assert.Empty(results);
    }

    [Fact]
    public async Task Subscribe_WithNullResolvedValue_ShouldHandleGracefully()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    nullStream
                }"
        };

        // When
        var subscription = executor.Subscribe(request, CancellationToken.None);
        var results = new List<ExecutionResult>();

        await foreach (var result in subscription)
        {
            results.Add(result);
        }

        // Then
        Assert.Empty(results);
    }

    [Fact]
    public async Task Subscribe_WithSlowStream_ShouldHandleBackpressure()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    slowStream
                }"
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // When
        var subscription = executor.Subscribe(request, cts.Token);
        var results = new List<ExecutionResult>();
        var startTime = DateTime.UtcNow;

        try
        {
            await foreach (var result in subscription)
            {
                results.Add(result);
                
                // Break after getting some results to avoid long test
                if (results.Count >= 3)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected if we hit the timeout
        }

        var duration = DateTime.UtcNow - startTime;

        // Then
        Assert.True(results.Count > 0);
        Assert.True(duration.TotalMilliseconds >= 100); // Should have taken some time due to delays
    }

    [Fact]
    public async Task Subscribe_WithMissingSubscriber_ShouldThrowException()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    missingSubscriber
                }"
        };

        // When & Then
        await Assert.ThrowsAsync<QueryException>(async () =>
        {
            await foreach (var result in executor.Subscribe(request, CancellationToken.None))
            {
                // Should throw before yielding any results
            }
        });
    }

    [Fact]
    public async Task Subscribe_WithSubscriberThrowingException_ShouldThrowFieldException()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    subscriberErrorStream
                }"
        };

        // When & Then
        await Assert.ThrowsAsync<FieldException>(async () =>
        {
            await foreach (var result in executor.Subscribe(request, CancellationToken.None))
            {
                // Should throw before yielding any results
            }
        });
    }

    [Fact]
    public async Task Subscribe_WithCancellationDuringExecution_ShouldCleanupProperly()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    count
                }"
        };

        using var cts = new CancellationTokenSource();
        var subscription = executor.Subscribe(request, cts.Token);
        var results = new List<ExecutionResult>();

        // When
        var enumerator = subscription.GetAsyncEnumerator(cts.Token);
        
        // Get first result
        Assert.True(await enumerator.MoveNextAsync());
        results.Add(enumerator.Current);

        // Cancel during execution
        cts.Cancel();

        // Then
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await enumerator.MoveNextAsync();
        });

        await enumerator.DisposeAsync();
        Assert.Single(results);
    }

    [Fact]
    public async Task Subscribe_WithNestedObjectErrors_ShouldHandleGracefully()
    {
        // Given
        var schema = await CreateTestSchema();
        var executor = new Executor(schema);
        var request = new GraphQLRequest
        {
            Query = @"
                subscription {
                    nestedObjectStream {
                        id
                        name
                        errorField
                    }
                }"
        };

        // When
        var subscription = executor.Subscribe(request, CancellationToken.None);
        var results = new List<ExecutionResult>();

        await foreach (var result in subscription.Take(3))
        {
            results.Add(result);
        }

        // Then
        Assert.All(results, result => Assert.NotNull(result.Data));
        Assert.All(results, result => Assert.NotNull(result.Errors));
        Assert.All(results, result => Assert.NotEmpty(result.Errors));
    }

    private static async Task<ISchema> CreateTestSchema()
    {
        return await new ExecutableSchemaBuilder()
            .Add("Query", new())
            .Add("Subscription", new()
            {
                {
                    "count: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "longRunningCount: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "errorStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "partialErrorStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "resolverErrorStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "emptyStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "nullStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "slowStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "missingSubscriber: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "subscriberErrorStream: Int!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                },
                {
                    "nestedObjectStream: [TestObject!]!", b => b.Run(ctx =>
                    {
                        ctx.ResolvedValue = ctx.ObjectValue;
                        return default;
                    })
                }
            },
            new()
            {
                {
                    "count: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = SimpleCountStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "longRunningCount: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = LongRunningCountStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "errorStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = ErrorStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "partialErrorStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = PartialErrorStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "resolverErrorStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = ResolverErrorStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "emptyStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = EmptyStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "nullStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = null;
                        return default;
                    })
                },
                {
                    "slowStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = SlowStream(cancellationToken);
                        return default;
                    })
                },
                {
                    "subscriberErrorStream: Int!", b => b.Run((ctx, cancellationToken) =>
                    {
                        throw new InvalidOperationException("Subscriber error");
                    })
                },
                {
                    "nestedObjectStream: [TestObject!]!", b => b.Run((ctx, cancellationToken) =>
                    {
                        ctx.ResolvedValue = NestedObjectStream(cancellationToken);
                        return default;
                    })
                }
            })
            .Add("TestObject", new()
            {
                {
                    "id: ID!", b => b.Run(ctx => 
                    {
                        ctx.ResolvedValue = ctx.ObjectValue?.GetType().GetProperty("Id")?.GetValue(ctx.ObjectValue);
                        return default;
                    })
                },
                {
                    "name: String!", b => b.Run(ctx => 
                    {
                        ctx.ResolvedValue = ctx.ObjectValue?.GetType().GetProperty("Name")?.GetValue(ctx.ObjectValue);
                        return default;
                    })
                },
                {
                    "errorField: String", b => b.Run(ctx => 
                    {
                        throw new InvalidOperationException("Resolver error");
                    })
                }
            })
            .Build();
    }

    private static async IAsyncEnumerable<object> SimpleCountStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 5; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
            await Task.Delay(50, cancellationToken);
        }
    }

    private static async IAsyncEnumerable<object> LongRunningCountStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
            await Task.Delay(50, cancellationToken);
        }
    }

    private static async IAsyncEnumerable<object> ErrorStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        throw new InvalidOperationException("Stream error");
        yield break; // Unreachable
    }

    private static async IAsyncEnumerable<object> PartialErrorStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return 1;
        await Task.Delay(50, cancellationToken);
        yield return 2;
        await Task.Delay(50, cancellationToken);
        throw new InvalidOperationException("Partial stream error");
    }

    private static async IAsyncEnumerable<object> ResolverErrorStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 5; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
            await Task.Delay(50, cancellationToken);
        }
    }

    private static async IAsyncEnumerable<object> EmptyStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        yield break;
    }

    private static async IAsyncEnumerable<object> SlowStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 10; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
            await Task.Delay(200, cancellationToken);
        }
    }

    private static async IAsyncEnumerable<object> NestedObjectStream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 5; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new TestObject { Id = i.ToString(), Name = $"Object {i}" };
            await Task.Delay(50, cancellationToken);
        }
    }

    public class TestObject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}