using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.TypeSystem;

using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Tests;

public class LinkDirectiveFilteringDebugTest
{
    private readonly ITestOutputHelper _output;

    public LinkDirectiveFilteringDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Debug_Link_Filtering_Process()
    {
        var schema = """
            extend schema @link(
              url: "https://mycompany.com/schemas/auth/v1.0",
              import: [
                { name: "@authenticated", as: "@requiresAuth" },
                { name: "@authorized", as: "@requiresPermission" }
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

        // Parse the schema
        var doc = Parser.Create(schema).ParseTypeSystemDocument();
        var linkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(doc.SchemaDefinitions, doc.SchemaExtensions);
        var linkInfo = linkInfos.First();

        _output.WriteLine($"Link Info: {linkInfo.Url}");
        _output.WriteLine($"Imports: {linkInfo.Imports?.Count ?? 0}");

        // Create a mock external schema
        var externalSchema = """
            directive @authenticated on FIELD_DEFINITION
            directive @authorized(role: String!) on FIELD_DEFINITION
            """;

        var externalDoc = Parser.Create(externalSchema).ParseTypeSystemDocument();
        _output.WriteLine($"External schema has {externalDoc.DirectiveDefinitions?.Count ?? 0} directives");

        if (externalDoc.DirectiveDefinitions != null)
        {
            foreach (var dir in externalDoc.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        // Create an existing types provider that simulates the builder state
        var existingTypesProvider = new TestExistingTypesProvider();

        // Apply import filtering
        var filteredDoc = LinkDirectiveProcessor.ApplyImportFiltering(externalDoc, linkInfo, existingTypesProvider);

        _output.WriteLine($"After filtering, have {filteredDoc.DirectiveDefinitions?.Count ?? 0} directives");
        if (filteredDoc.DirectiveDefinitions != null)
        {
            foreach (var dir in filteredDoc.DirectiveDefinitions)
            {
                _output.WriteLine($"  - {dir.Name.Value}");
            }
        }

        Assert.True(filteredDoc.DirectiveDefinitions?.Count > 0, "Should have imported directives");
    }

    private class TestExistingTypesProvider : IExistingTypesProvider
    {
        public bool ContainsType(string typeName) => false;
        public bool ContainsDirective(string directiveName) => false;
    }
}