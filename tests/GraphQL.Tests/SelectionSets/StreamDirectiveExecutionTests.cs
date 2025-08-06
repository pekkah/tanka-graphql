using System;
using System.Collections.Generic;
using System.Linq;
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

public class StreamDirectiveExecutionTests
{
    [Fact]
    public async Task Stream_Should_Return_Initial_Items_And_Stream_Remaining()
    {
        // Given - Schema with products that can be streamed
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                {
                    "products: [Product]",
                    b => b.ResolveAs(new[]
                    {
                        new ProductModel { id = "1", name = "Product 1" },
                        new ProductModel { id = "2", name = "Product 2" },
                        new ProductModel { id = "3", name = "Product 3" },
                        new ProductModel { id = "4", name = "Product 4" },
                        new ProductModel { id = "5", name = "Product 5" }
                    })
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
                query GetProducts {
                    products @stream(initialCount: 2) {
                        id
                        name
                    }
                }
            """
        };

        // Configure services with incremental delivery support
        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute the query with @stream directive
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Then - Should have initial result plus incremental results
        Assert.True(results.Count > 1, "Should have multiple results due to streaming");

        // First result should have initial items (2) and hasNext: true
        var firstResult = results.First();
        Assert.NotNull(firstResult.Data);
        Assert.True(firstResult.HasNext);

        var products = firstResult.Data["products"] as List<object>;
        Assert.NotNull(products);
        Assert.Equal(2, products.Count);

        // Subsequent results should have incremental data
        var hasIncrementalResults = results.Skip(1).Any(r => r.Incremental?.Any() == true);
        Assert.True(hasIncrementalResults, "Should have incremental results with stream items");

        // Last result should have hasNext: false
        var lastResult = results.Last();
        Assert.False(lastResult.HasNext);
    }

    [Fact]
    public async Task Stream_With_InitialCount_Zero_Should_Stream_All_Items()
    {
        // Given - Schema with items that can be streamed
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
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
                    items @stream(initialCount: 0)
                }
            """
        };

        // Configure services with incremental delivery support
        var services = new ServiceCollection();
        services.AddDefaultTankaGraphQLServices();
        services.AddIncrementalDeliveryDirectives();
        var serviceProvider = services.BuildServiceProvider();

        // When - Execute with initialCount: 0
        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = serviceProvider });
        var queryContext = executor.BuildQueryContextAsync(request);
        var stream = executor.ExecuteOperation(queryContext);

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Then - Should have initial empty result plus all items streamed
        Assert.True(results.Count > 1, "Should have multiple results due to streaming");

        // First result should have empty items array
        var firstResult = results.First();
        Assert.NotNull(firstResult.Data);
        Assert.True(firstResult.HasNext);

        var items = firstResult.Data["items"] as List<object>;
        Assert.NotNull(items);
        Assert.Empty(items);

        // Should have incremental results for each item
        var incrementalResults = results.Skip(1).Where(r => r.Incremental?.Any() == true).ToList();
        Assert.True(incrementalResults.Count >= 3, "Should have at least 3 incremental results for the items");
    }

    private class ProductModel
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
    }
}