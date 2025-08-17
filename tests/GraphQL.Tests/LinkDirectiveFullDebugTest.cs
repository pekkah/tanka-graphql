using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.TypeSystem;

using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Tests;

public class LinkDirectiveFullDebugTest
{
    private readonly ITestOutputHelper _output;

    public LinkDirectiveFullDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Debug_Full_Link_Pipeline_With_Aliasing()
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

        // Step 1: Parse schema and extract link info
        var doc = Parser.Create(schema).ParseTypeSystemDocument();
        var linkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(doc.SchemaDefinitions, doc.SchemaExtensions);
        var linkInfo = linkInfos.First();

        _output.WriteLine("=== STEP 1: Link Info Extraction ===");
        _output.WriteLine($"URL: {linkInfo.Url}");
        _output.WriteLine($"Imports: {linkInfo.Imports?.Count ?? 0}");
        if (linkInfo.Imports != null)
        {
            foreach (var import in linkInfo.Imports)
            {
                _output.WriteLine($"  - Source: {import.SourceName}, Alias: {import.Alias}, Effective: {import.EffectiveName}");
            }
        }

        // Step 2: Load external schema
        var schemaLoader = new TestAuthSchemaLoader();
        var externalDoc = await schemaLoader.LoadSchemaAsync(linkInfo.Url);

        _output.WriteLine("\n=== STEP 2: External Schema ===");
        _output.WriteLine($"External directives: {externalDoc?.DirectiveDefinitions?.Count ?? 0}");
        if (externalDoc?.DirectiveDefinitions != null)
        {
            foreach (var dir in externalDoc.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        // Step 3: Create existing types provider (empty for our test)
        var existingTypesProvider = new EmptyTypesProvider();

        // Step 4: Apply import filtering
        _output.WriteLine("\n=== STEP 3: Import Filtering ===");
        var filteredDoc = LinkDirectiveProcessor.ApplyImportFiltering(externalDoc!, linkInfo, existingTypesProvider);

        _output.WriteLine($"Filtered directives: {filteredDoc.DirectiveDefinitions?.Count ?? 0}");
        if (filteredDoc.DirectiveDefinitions != null)
        {
            foreach (var dir in filteredDoc.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        // Step 5A: Test LoadAndResolveSchemaAsync simulation
        _output.WriteLine("\n=== STEP 4A: LoadAndResolveSchemaAsync Simulation ===");
        var rawLoadedSchema = await schemaLoader.LoadSchemaAsync(linkInfo.Url);
        _output.WriteLine($"Raw loaded schema directives: {rawLoadedSchema?.DirectiveDefinitions?.Count ?? 0}");

        // Test if our external schema has nested @link directives
        var nestedLinkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(
            rawLoadedSchema?.SchemaDefinitions,
            rawLoadedSchema?.SchemaExtensions);
        _output.WriteLine($"Nested link infos in external schema: {nestedLinkInfos.Count()}");

        // Apply filtering with SimpleExistingTypesProvider (like LoadAndResolveSchemaAsync does)
        var simpleProvider = new SimpleExistingTypesProvider();
        var directlyFiltered = LinkDirectiveProcessor.ApplyImportFiltering(rawLoadedSchema!, linkInfo, simpleProvider);
        _output.WriteLine($"Directly filtered directives (with SimpleProvider): {directlyFiltered.DirectiveDefinitions?.Count ?? 0}");
        if (directlyFiltered.DirectiveDefinitions != null)
        {
            foreach (var dir in directlyFiltered.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        // Step 5B: Test the full resolution process
        _output.WriteLine("\n=== STEP 4B: Full Resolution ===");
        var resolvedDoc = await LinkDirectiveProcessor.ResolveLinkedSchemasAsync(
            linkInfos,
            schemaLoader,
            existingTypesProvider,
            10);

        // Step 5C: Test with SchemaBuilderTypesProvider (like the real scenario)
        _output.WriteLine("\n=== STEP 4C: Test with SchemaBuilderTypesProvider ===");
        var mockBuilder = new SchemaBuilder();
        mockBuilder.Add(doc);
        var schemaBuilderProvider = new MockSchemaBuilderTypesProvider(mockBuilder);
        var resolvedDocWithBuilder = await LinkDirectiveProcessor.ResolveLinkedSchemasAsync(
            linkInfos,
            schemaLoader,
            schemaBuilderProvider,
            10);

        _output.WriteLine($"Resolved directives (with SchemaBuilderProvider): {resolvedDocWithBuilder?.DirectiveDefinitions?.Count ?? 0}");
        if (resolvedDocWithBuilder?.DirectiveDefinitions != null)
        {
            foreach (var dir in resolvedDocWithBuilder.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        _output.WriteLine($"Resolved directives: {resolvedDoc?.DirectiveDefinitions?.Count ?? 0}");
        if (resolvedDoc?.DirectiveDefinitions != null)
        {
            foreach (var dir in resolvedDoc.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        // Step 6: Test with SchemaBuilder
        _output.WriteLine("\n=== STEP 5: Schema Builder ===");
        var builder = new SchemaBuilder();
        builder.Add(doc);

        var options = new SchemaBuildOptions();
        options.SchemaLoader = schemaLoader;

        await builder.ProcessLinkDirectivesAsync(options);

        _output.WriteLine($"Builder directives after processing: {builder.GetDirectiveDefinitions().Count()}");
        foreach (var dir in builder.GetDirectiveDefinitions())
        {
            _output.WriteLine($"  - {dir.Name}");
        }

        // Verify that the filtering step works correctly
        Assert.True(filteredDoc.DirectiveDefinitions?.Count > 0, "Import filtering should produce directives");
        Assert.True(filteredDoc.DirectiveDefinitions?.Any(d => d.Name.Value == "requiresAuth"), "Should have requiresAuth directive");
    }

    private class EmptyTypesProvider : IExistingTypesProvider
    {
        public bool ContainsType(string typeName)
        {
            return false;
        }

        public bool ContainsDirective(string directiveName)
        {
            return false;
        }
    }

    private class SimpleExistingTypesProvider : IExistingTypesProvider
    {
        public bool ContainsType(string typeName) => false;
        public bool ContainsDirective(string directiveName) => false;
    }

    private class MockSchemaBuilderTypesProvider : IExistingTypesProvider
    {
        private readonly SchemaBuilder _builder;

        public MockSchemaBuilderTypesProvider(SchemaBuilder builder)
        {
            _builder = builder;
        }

        public bool ContainsType(string typeName) => _builder.GetTypeDefinitions().Any(t => t.Name == typeName);
        public bool ContainsDirective(string directiveName) => _builder.GetDirectiveDefinitions().Any(d => d.Name == directiveName);
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

            return Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>((Language.Nodes.TypeSystem.TypeSystemDocument)authSchema);
        }
    }
}