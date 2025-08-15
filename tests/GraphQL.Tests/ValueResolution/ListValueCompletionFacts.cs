using System;
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

namespace Tanka.GraphQL.Tests.ValueResolution;

public class ListValueCompletionFacts
{
    [Fact]
    public async Task CompleteListValue_Should_Handle_IEnumerable()
    {
        // Given - Schema with regular IEnumerable field
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "items: [String]",
                    b => b.ResolveAs(new[] { "item1", "item2", "item3" })
                }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetItems {
                    items
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with IEnumerable resolver
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - Should complete successfully as a list
        Assert.NotNull(result.Data);
        var items = result.Data["items"] as List<object>;
        Assert.NotNull(items);
        Assert.Equal(3, items.Count);
        Assert.Equal("item1", items[0]);
        Assert.Equal("item2", items[1]);
        Assert.Equal("item3", items[2]);
    }

    [Fact]
    public async Task CompleteListValue_Should_Handle_IAsyncEnumerable_Without_Stream_Directive()
    {
        // Given - Schema with IAsyncEnumerable field but NO @stream directive
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "asyncItems: [Product]",
                    b => b.ResolveAs(GetProductsAsync())
                }
            })
            .Add("Product", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.id) },
                { "name: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.name) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetAsyncItems {
                    asyncItems {
                        id
                        name
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with IAsyncEnumerable resolver but NO @stream
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - Should fully enumerate the async enumerable and return as complete list
        Assert.NotNull(result.Data);
        var asyncItems = result.Data["asyncItems"] as List<object>;
        Assert.NotNull(asyncItems);
        Assert.Equal(3, asyncItems.Count);

        // Should not have hasNext or incremental data (no streaming)
        Assert.Null(result.HasNext);
        Assert.Null(result.Incremental);

        // Verify the data is correct
        var firstItem = asyncItems[0] as Dictionary<string, object>;
        Assert.NotNull(firstItem);
        Assert.Equal("prod-1", firstItem["id"]);
        Assert.Equal("Product 1", firstItem["name"]);
    }

    [Fact]
    public async Task CompleteListValue_Should_Handle_IAsyncEnumerable_With_Stream_Directive()
    {
        // Given - Schema with IAsyncEnumerable field WITH @stream directive
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                {
                    "asyncItems: [Product]",
                    b => b.ResolveAs(GetProductsAsync())
                }
            })
            .Add("Product", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.id) },
                { "name: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.name) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetAsyncItems {
                    asyncItems @stream(initialCount: 1) {
                        id
                        name
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with IAsyncEnumerable resolver WITH @stream
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Then - Should stream items as they become available
        Assert.True(results.Count > 1, "Should have multiple results due to streaming");

        // First result should have initial item and hasNext: true
        var firstResult = results.First();
        Assert.NotNull(firstResult.Data);
        Assert.True(firstResult.HasNext);

        var initialItems = firstResult.Data["asyncItems"] as List<object>;
        Assert.NotNull(initialItems);
        Assert.Single(initialItems);

        // Should have incremental results for remaining items
        var incrementalResults = results.Skip(1).Where(r => r.Incremental?.Any() == true).ToList();
        Assert.True(incrementalResults.Any(), "Should stream remaining items incrementally");

        // Last result should indicate no more data
        var lastResult = results.Last();
        Assert.False(lastResult.HasNext);
    }

    [Fact]
    public async Task CompleteListValue_Should_Handle_Empty_IAsyncEnumerable_Without_Stream()
    {
        // Given - Schema with empty IAsyncEnumerable field
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "emptyAsyncItems: [Product]",
                    b => b.ResolveAs(GetEmptyProductsAsync())
                }
            })
            .Add("Product", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.id) },
                { "name: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.name) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetEmptyAsyncItems {
                    emptyAsyncItems {
                        id
                        name
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with empty IAsyncEnumerable
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - Should return empty list
        Assert.NotNull(result.Data);
        var emptyItems = result.Data["emptyAsyncItems"] as List<object>;
        Assert.NotNull(emptyItems);
        Assert.Empty(emptyItems);

        // Should not have streaming data
        Assert.Null(result.HasNext);
        Assert.Null(result.Incremental);
    }

    [Fact]
    public async Task CompleteListValue_Should_Handle_IAsyncEnumerable_With_Exceptions()
    {
        // Given - Schema with IAsyncEnumerable that throws during enumeration
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "faultyAsyncItems: [Product]",
                    b => b.ResolveAs(GetFaultyProductsAsync())
                }
            })
            .Add("Product", new()
            {
                { "id: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.id) },
                { "name: String", b => b.ResolveAsPropertyOf<ProductModel>(p => p.name) }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetFaultyAsyncItems {
                    faultyAsyncItems {
                        id
                        name
                    }
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with faulty IAsyncEnumerable
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - Should handle exception gracefully
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Any(), "Should have errors from async enumerable failure");
    }

    [Fact]
    public async Task CompleteListValue_Should_Handle_String_As_IEnumerable_Of_Chars()
    {
        // Given - Schema with field that returns string for list field
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "stringAsCollection: [String]",
                    b => b.ResolveAs("abc")
                }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetStringAsCollection {
                    stringAsCollection
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with string resolver for list field
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - String should be enumerated as characters (because string implements IEnumerable<char>)
        Assert.NotNull(result.Data);
        var stringAsCollection = result.Data["stringAsCollection"] as List<object>;
        Assert.NotNull(stringAsCollection);
        Assert.Equal(3, stringAsCollection.Count);
        Assert.Equal("a", stringAsCollection[0]);
        Assert.Equal("b", stringAsCollection[1]);
        Assert.Equal("c", stringAsCollection[2]);
    }

    [Fact]
    public async Task CompleteListValue_Should_Throw_For_True_Non_Collection_Types()
    {
        // Given - Schema with field that returns true non-collection type (int) for list field
        var schema = await new ExecutableSchemaBuilder()
            .Add("Query", new()
            {
                {
                    "notACollection: [String]",
                    b => b.ResolveAs(42) // int is not IEnumerable or IAsyncEnumerable
                }
            })
            .Build();

        var request = new GraphQLRequest()
        {
            Query = """
                query GetNonCollection {
                    notACollection
                }
            """
        };

        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute query with true non-collection resolver for list field
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);
        var result = await stream.FirstAsync();

        // Then - Should have error about value not being a collection
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Message.Contains("Resolved value is not a collection"));
    }

    // Helper methods to generate test data

    private static async IAsyncEnumerable<ProductModel> GetProductsAsync()
    {
        var products = new[]
        {
            new ProductModel { id = "prod-1", name = "Product 1" },
            new ProductModel { id = "prod-2", name = "Product 2" },
            new ProductModel { id = "prod-3", name = "Product 3" }
        };

        foreach (var product in products)
        {
            // Simulate async operation
            await Task.Delay(5);
            yield return product;
        }
    }

    private static async IAsyncEnumerable<ProductModel> GetEmptyProductsAsync()
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<ProductModel> GetFaultyProductsAsync()
    {
        yield return new ProductModel { id = "prod-1", name = "Product 1" };
        await Task.Delay(5);
        throw new InvalidOperationException("Simulated async enumerable failure");
    }

    private class ProductModel
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
    }
}