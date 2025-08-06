using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tests.SelectionSets;

public class AsyncStreamDirectiveExecutionTests
{
    [Fact]
    public async Task Stream_Should_Support_True_Async_Streaming_With_IAsyncEnumerable()
    {
        // Given - Schema with async enumerable field that truly streams data
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                {
                    "events: [Event]",
                    b => b.ResolveAs(GetEventsAsync())
                }
            })
            .Add("Event", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<EventModel>(e => e.id) },
                { "message: String", b => b.ResolveAsPropertyOf<EventModel>(e => e.message) },
                { "timestamp: Int", b => b.ResolveAsPropertyOf<EventModel>(e => e.timestamp) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query StreamEvents {
                    events @stream(initialCount: 2) {
                        id
                        message
                        timestamp
                    }
                }
            """
        };

        // Configure services with incremental delivery support
        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute the query with @stream directive on async enumerable
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Then - Should stream items as they become available
        Assert.True(results.Count >= 2, "Should have initial result plus streamed results");

        // First result should have initial 2 items
        var firstResult = results.First();
        Assert.NotNull(firstResult.Data);
        Assert.True(firstResult.HasNext);

        var events = firstResult.Data["events"] as List<object>;
        Assert.NotNull(events);
        Assert.Equal(2, events.Count);

        // Should have incremental results for remaining items
        var incrementalResults = results.Skip(1).Where(r => r.Incremental?.Any() == true).ToList();
        Assert.True(incrementalResults.Count >= 1, "Should stream remaining events incrementally");

        // Last result should indicate no more data
        var lastResult = results.Last();
        Assert.False(lastResult.HasNext);
    }

    [Fact]
    public async Task Stream_Should_Handle_Empty_AsyncEnumerable()
    {
        // Given - Schema with async enumerable that returns no items
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                {
                    "events: [Event]",
                    b => b.ResolveAs(GetEmptyEventsAsync())
                }
            })
            .Add("Event", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<EventModel>(e => e.id) },
                { "message: String", b => b.ResolveAsPropertyOf<EventModel>(e => e.message) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query StreamEvents {
                    events @stream(initialCount: 5) {
                        id
                        message
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute with empty async enumerable
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Then - Should handle empty stream gracefully
        Assert.Single(results);

        var singleResult = results[0];
        Assert.NotNull(singleResult.Data);
        // For empty async enumerable, HasNext should be null since no incremental delivery was set up
        Assert.Null(singleResult.HasNext);

        var events = singleResult.Data["events"] as List<object>;
        Assert.NotNull(events);
        Assert.Empty(events);
    }

    [Fact]
    public async Task Stream_Should_Handle_Cancellation_Of_AsyncEnumerable()
    {
        // Given - Schema with cancellable async enumerable
        using var cts = new CancellationTokenSource();

        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                {
                    "events: [Event]",
                    b => b.ResolveAs(GetCancellableEventsAsync(cts.Token))
                }
            })
            .Add("Event", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<EventModel>(e => e.id) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query StreamEvents {
                    events @stream(initialCount: 1) {
                        id
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Start execution then cancel
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();

        // Collect first result
        await foreach (var result in stream)
        {
            results.Add(result);

            // Cancel after first result
            if (results.Count == 1)
            {
                cts.Cancel();
                break;
            }
        }

        // Then - Should handle cancellation gracefully
        Assert.Single(results);
        var firstResult = results[0];
        Assert.NotNull(firstResult.Data);
        // Note: Cancellation may or may not set HasNext to false depending on timing
    }

    // Helper methods to generate async enumerables

    private static async IAsyncEnumerable<EventModel> GetEventsAsync()
    {
        for (int i = 1; i <= 5; i++)
        {
            // Simulate async data fetching with small delay
            await Task.Delay(10);
            yield return new EventModel
            {
                id = $"event-{i}",
                message = $"Event {i} occurred",
                timestamp = i * 1000
            };
        }
    }

    private static async IAsyncEnumerable<EventModel> GetEmptyEventsAsync()
    {
        // Return empty async enumerable
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<EventModel> GetCancellableEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 1; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(50, cancellationToken);
            yield return new EventModel
            {
                id = $"event-{i}",
                message = $"Event {i}",
                timestamp = i * 1000
            };
        }
    }

    private class EventModel
    {
        public string id { get; set; } = "";
        public string message { get; set; } = "";
        public int timestamp { get; set; }
    }
}