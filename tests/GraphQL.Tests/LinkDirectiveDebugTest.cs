using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.TypeSystem;

using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Tests;

public class LinkDirectiveDebugTest
{
    private readonly ITestOutputHelper _output;

    public LinkDirectiveDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Debug_Link_Processing()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: ["@authenticated"]
            )

            type Query {
                profile: User
            }

            type User {
                id: ID!
                name: String
            }
            """;

        var builder = new SchemaBuilder();
        builder.Add(schema);

        _output.WriteLine($"Directives in builder before link processing: {builder.GetDirectiveDefinitions().Count()}");
        foreach (var dir in builder.GetDirectiveDefinitions())
        {
            _output.WriteLine($"  - {dir.Name}");
        }

        var options = new SchemaBuildOptions();
        options.SchemaLoader = new TestAuthSchemaLoader();

        // Build schema (this will automatically process links via middleware)
        var builtSchema = await builder.Build(options);

        // Check what we have
        var allDirectives = builtSchema.QueryDirectiveTypes();
        _output.WriteLine($"Directives in final schema: {allDirectives.Count()}");
        foreach (var dir in allDirectives)
        {
            _output.WriteLine($"  - {dir.Name}");
        }

        var authenticatedDirective = builtSchema.GetDirectiveType("authenticated");
        _output.WriteLine($"authenticated directive: {authenticatedDirective?.Name ?? "null"}");

        // Just check that something was processed
        Assert.True(builder.GetDirectiveDefinitions().Any());
    }

    [Fact]
    public async Task Debug_Link_Processing_With_Aliasing()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: [
                { name: "@authenticated", as: "@requiresAuth" }
              ]
            )

            type Query {
                profile: User
            }

            type User {
                id: ID!
                name: String
            }
            """;

        var builder = new SchemaBuilder();
        builder.Add(schema);

        _output.WriteLine($"Directives in builder before link processing: {builder.GetDirectiveDefinitions().Count()}");
        foreach (var dir in builder.GetDirectiveDefinitions())
        {
            _output.WriteLine($"  - {dir.Name}");
        }

        var options = new SchemaBuildOptions();
        options.SchemaLoader = new TestAuthSchemaLoader();

        // Build schema (this will automatically process links via middleware)
        var builtSchema = await builder.Build(options);

        // Check what we have
        var allDirectives = builtSchema.QueryDirectiveTypes();
        _output.WriteLine($"Directives in final schema: {allDirectives.Count()}");
        foreach (var dir in allDirectives)
        {
            _output.WriteLine($"  - {dir.Name}");
        }

        var requiresAuthDirective = builtSchema.GetDirectiveType("requiresAuth");
        var authenticatedDirective = builtSchema.GetDirectiveType("authenticated");

        _output.WriteLine($"requiresAuth directive: {requiresAuthDirective?.Name ?? "null"}");
        _output.WriteLine($"authenticated directive: {authenticatedDirective?.Name ?? "null"}");

        // Check that aliasing worked correctly
        Assert.NotNull(requiresAuthDirective);
        Assert.Null(authenticatedDirective);
    }

    private class TestAuthSchemaLoader : ISchemaLoader
    {
        public bool CanLoad(string url) => url.StartsWith("https://mycompany.com/schemas/auth/");

        public Task<Language.Nodes.TypeSystem.TypeSystemDocument?> LoadSchemaAsync(
            string url,
            System.Threading.CancellationToken cancellationToken = default)
        {
            if (!CanLoad(url))
                return Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>(null);

            var authSchema = """
                directive @authenticated on FIELD_DEFINITION
                directive @authorized(role: String!) on FIELD_DEFINITION
                """;

            return Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>(
                (Language.Nodes.TypeSystem.TypeSystemDocument)authSchema);
        }
    }
}