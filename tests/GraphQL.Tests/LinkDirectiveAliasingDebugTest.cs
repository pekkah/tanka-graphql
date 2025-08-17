using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.TypeSystem;

using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Tests;

public class LinkDirectiveAliasingDebugTest
{
    private readonly ITestOutputHelper _output;

    public LinkDirectiveAliasingDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Debug_Link_Directive_Import_Parsing()
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

        var doc = Parser.Create(schema).ParseTypeSystemDocument();
        _output.WriteLine($"Parsed document with {doc.SchemaExtensions?.Count ?? 0} schema extensions");

        var schemaExtension = doc.SchemaExtensions?.FirstOrDefault();
        var linkDirective = schemaExtension?.Directives?.FirstOrDefault(d => d.Name.Value == "link");

        if (linkDirective != null)
        {
            _output.WriteLine($"Found @link directive with {linkDirective.Arguments?.Count ?? 0} arguments");

            if (linkDirective.Arguments != null)
            {
                foreach (var arg in linkDirective.Arguments)
                {
                    _output.WriteLine($"  Argument: {arg.Name.Value} = {arg.Value}");
                }
            }

            // Extract links using the LinkDirectiveProcessor
            var linkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(doc.SchemaDefinitions, doc.SchemaExtensions);
            _output.WriteLine($"Extracted {linkInfos.Count()} link directives");

            foreach (var linkInfo in linkInfos)
            {
                _output.WriteLine($"Link: {linkInfo.Url}");
                _output.WriteLine($"  Imports: {linkInfo.Imports?.Count ?? 0}");
                if (linkInfo.Imports != null)
                {
                    foreach (var import in linkInfo.Imports)
                    {
                        _output.WriteLine($"    - Source: {import.SourceName}, Alias: {import.Alias}, Effective: {import.EffectiveName}");
                    }
                }
            }
        }
        else
        {
            _output.WriteLine("No @link directive found");
        }

        Assert.True(linkDirective != null, "Should find @link directive");
    }
}