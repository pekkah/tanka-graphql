using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

using Xunit;

namespace Tanka.GraphQL.Tutorials.GettingStarted;

/// <summary>
/// Tutorial tests demonstrating @defer and @stream directives for incremental delivery.
/// These examples focus on learning concepts with simple, readable scenarios.
/// </summary>
public class DeferStreamTutorials
{
    [Fact]
    public async Task Defer_Tutorial_Basic_Usage()
    {
        // @defer allows you to mark parts of a query as non-critical, 
        // so they can be delivered after the initial response

        // 1. Create schema with fast and slow fields
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives() // Enable @defer and @stream
            .Add("Query", new()
            {
                { "user: User", b => b.ResolveAs(new { id = "123", name = "Alice" }) }
            })
            .Add("User", new()
            {
                // Fast field - returns immediately
                { "id: String", (dynamic user) => user.id },
                { "name: String", (dynamic user) => user.name },
                
                // Slow field - simulates expensive operation
                { "profile: UserProfile", async (ResolverContext context) =>
                {
                    await Task.Delay(500); // Simulate database lookup
                    context.ResolvedValue = new { email = "alice@example.com", bio = "Software Engineer" };
                }}
            })
            .Add("UserProfile", new()
            {
                { "email: String", (dynamic profile) => profile.email },
                { "bio: String", (dynamic profile) => profile.bio }
            })
            .Build();

        // 2. Configure services for incremental delivery
        var services = new ServiceCollection()
            .AddDefaultTankaGraphQLServices()
            .AddIncrementalDeliveryDirectives()
            .BuildServiceProvider();

        // 3. Execute query with @defer directive
        var query = """
            {
                user {
                    id
                    name
                    ... @defer(label: "profile") {
                        profile {
                            email
                            bio
                        }
                    }
                }
            }
        """;

        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = services });
        var queryContext = executor.BuildQueryContextAsync(new GraphQLRequest { Query = query });
        var stream = executor.ExecuteOperation(queryContext);

        // 4. Collect streaming results
        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // 5. Verify incremental delivery
        Assert.True(results.Count >= 2, "Should have initial result plus deferred result");

        // First result contains fast fields only
        var initialResult = results[0];
        Assert.True(initialResult.HasNext);
        Assert.Contains("user", initialResult.Data.Keys);

        // Subsequent results contain deferred fields
        var hasProfileData = results.Skip(1).Any(r =>
            r.Incremental?.Any(inc => inc.Label == "profile") == true);
        Assert.True(hasProfileData, "Should have deferred profile data");
    }

    [Fact]
    public async Task Stream_Tutorial_Basic_Usage()
    {
        // @stream allows you to deliver list items incrementally
        // instead of waiting for the entire list to be ready

        // 1. Create schema with a list field that can be streamed
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                { "books: [Book]", async (ResolverContext context) =>
                {
                    // Simulate fetching books one by one from different sources
                    var books = new List<object>();
                    for (int i = 1; i <= 5; i++)
                    {
                        await Task.Delay(100); // Simulate individual fetch delay
                        books.Add(new { id = $"book-{i}", title = $"Book {i}", pages = i * 100 });
                    }
                    context.ResolvedValue = books;
                }}
            })
            .Add("Book", new()
            {
                { "id: String", (dynamic book) => book.id },
                { "title: String", (dynamic book) => book.title },
                { "pages: Int", (dynamic book) => book.pages }
            })
            .Build();

        // 2. Configure services
        var services = new ServiceCollection()
            .AddDefaultTankaGraphQLServices()
            .AddIncrementalDeliveryDirectives()
            .BuildServiceProvider();

        // 3. Execute query with @stream directive
        var query = """
            {
                books @stream(initialCount: 2) {
                    id
                    title
                    pages
                }
            }
        """;

        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = services });
        var queryContext = executor.BuildQueryContextAsync(new GraphQLRequest { Query = query });
        var stream = executor.ExecuteOperation(queryContext);

        // 4. Collect streaming results
        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // 5. Verify streaming behavior
        Assert.True(results.Count >= 2, "Should have initial result plus streamed items");

        // First result should have initial items (2 books)
        var initialResult = results[0];
        Assert.True(initialResult.HasNext);
        var initialBooks = initialResult.Data["books"] as List<object>;
        Assert.NotNull(initialBooks);
        Assert.Equal(2, initialBooks.Count);

        // Should have incremental results for remaining items
        var hasStreamedItems = results.Skip(1).Any(r => r.Incremental?.Any() == true);
        Assert.True(hasStreamedItems, "Should have streamed the remaining books");
    }

    [Fact]
    public async Task Combined_Defer_And_Stream_Tutorial()
    {
        // You can combine @defer and @stream in the same query
        // for maximum flexibility in data delivery

        // 1. Create schema with both deferred and streamable fields
        var schema = await new ExecutableSchemaBuilder()
            .AddIncrementalDeliveryDirectives()
            .Add("Query", new()
            {
                { "library: Library", (ResolverContext context) =>
                {
                    context.ResolvedValue = new { name = "City Library", established = 1950 };
                }}
            })
            .Add("Library", new()
            {
                // Fast fields
                { "name: String", (dynamic library) => library.name },
                { "established: Int", (dynamic library) => library.established },
                
                // Slow streamable field
                { "books: [Book]", async (ResolverContext context) =>
                {
                    await Task.Delay(200); // Simulate catalog lookup
                    var books = new List<object>
                    {
                        new { id = "1", title = "GraphQL Guide", category = "Tech" },
                        new { id = "2", title = "API Design", category = "Tech" },
                        new { id = "3", title = "System Architecture", category = "Tech" }
                    };
                    context.ResolvedValue = books;
                }},
                
                // Slow deferred field
                { "stats: LibraryStats", async (ResolverContext context) =>
                {
                    await Task.Delay(800); // Simulate analytics calculation
                    context.ResolvedValue = new { totalBooks = 50000, dailyVisitors = 250 };
                }}
            })
            .Add("Book", new()
            {
                { "id: String", (dynamic book) => book.id },
                { "title: String", (dynamic book) => book.title },
                { "category: String", (dynamic book) => book.category }
            })
            .Add("LibraryStats", new()
            {
                { "totalBooks: Int", (dynamic stats) => stats.totalBooks },
                { "dailyVisitors: Int", (dynamic stats) => stats.dailyVisitors }
            })
            .Build();

        // 2. Configure services
        var services = new ServiceCollection()
            .AddDefaultTankaGraphQLServices()
            .AddIncrementalDeliveryDirectives()
            .BuildServiceProvider();

        // 3. Execute query combining @defer and @stream
        var query = """
            {
                library {
                    name
                    established
                    books @stream(initialCount: 1) {
                        id
                        title
                        category
                    }
                    ... @defer(label: "stats") {
                        stats {
                            totalBooks
                            dailyVisitors
                        }
                    }
                }
            }
        """;

        var executor = new Executor(new ExecutorOptions { Schema = schema, ServiceProvider = services });
        var queryContext = executor.BuildQueryContextAsync(new GraphQLRequest { Query = query });
        var stream = executor.ExecuteOperation(queryContext);

        // 4. Collect all results
        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // 5. Verify combined behavior
        Assert.True(results.Count >= 3, "Should have initial + streamed books + deferred stats");

        // Initial result has basic info and first book
        var initialResult = results[0];
        Assert.True(initialResult.HasNext);
        Assert.Contains("library", initialResult.Data.Keys);

        // Should have both streamed books and deferred stats in subsequent results
        var hasStreamedBooks = results.Any(r => r.Incremental?.Any() == true);
        var hasDeferredStats = results.Any(r => r.Incremental?.Any(inc => inc.Label == "stats") == true);

        Assert.True(hasStreamedBooks, "Should have streamed book items");
        Assert.True(hasDeferredStats, "Should have deferred stats");
    }
}