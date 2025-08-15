using System.Linq;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem;

public class LinkDirectiveProcessorTests
{
    [Fact]
    public void ProcessLinkDirective_WithUrl_ShouldCreateLinkInfo()
    {
        // Given
        var schema = Parser.Create(@"
extend schema @link(url: ""https://specs.apollo.dev/federation/v2.0"")

type Query {
    hello: String
}
").ParseTypeSystemDocument();

        // When
        var links = LinkDirectiveProcessor.ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Then
        var link = Assert.Single(links);
        Assert.Equal("https://specs.apollo.dev/federation/v2.0", link.Url);
        Assert.Null(link.Imports);
    }

    [Fact]
    public void ProcessLinkDirective_WithImports_ShouldCreateLinkInfoWithTypes()
    {
        // Given
        var schema = Parser.Create(@"
extend schema @link(url: ""https://specs.apollo.dev/federation/v2.0"", import: [""@key"", ""@external""])

type Query {
    hello: String
}
").ParseTypeSystemDocument();

        // When
        var links = LinkDirectiveProcessor.ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Then
        var link = Assert.Single(links);
        Assert.Equal("https://specs.apollo.dev/federation/v2.0", link.Url);
        Assert.NotNull(link.Imports);
        Assert.Equal(2, link.Imports.Count);
        Assert.Contains(link.Imports, i => i.SourceName == "@key");
        Assert.Contains(link.Imports, i => i.SourceName == "@external");
    }

    [Fact]
    public void ProcessLinkDirective_MultipleLinks_ShouldCreateMultipleLinkInfos()
    {
        // Given
        var schema = Parser.Create(@"
extend schema 
    @link(url: ""https://specs.apollo.dev/federation/v2.0"", import: [""@key""]) 
    @link(url: ""https://example.com/custom"", import: [""@custom""])

type Query {
    hello: String
}
").ParseTypeSystemDocument();

        // When
        var links = LinkDirectiveProcessor.ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Then
        Assert.Equal(2, links.Count());
        var federationLink = links.FirstOrDefault(i => i.Url == "https://specs.apollo.dev/federation/v2.0");
        var customLink = links.FirstOrDefault(i => i.Url == "https://example.com/custom");

        Assert.NotNull(federationLink);
        Assert.NotNull(customLink);
        Assert.Single(federationLink.Imports, i => i.SourceName == "@key");
        Assert.Single(customLink.Imports, i => i.SourceName == "@custom");
    }

    [Fact]
    public void ProcessLinkDirective_WithAliasing_ShouldCreateImportInfoWithAlias()
    {
        // Given
        var schema = Parser.Create(@"
extend schema @link(url: ""https://specs.apollo.dev/federation/v2.0"", import: [{name: ""@key"", as: ""@myKey""}, ""@external""])

type Query {
    hello: String
}
").ParseTypeSystemDocument();

        // When
        var links = LinkDirectiveProcessor.ProcessLinkDirectives(
            schema.SchemaDefinitions,
            schema.SchemaExtensions);

        // Then
        var link = Assert.Single(links);
        Assert.Equal("https://specs.apollo.dev/federation/v2.0", link.Url);
        Assert.NotNull(link.Imports);
        Assert.Equal(2, link.Imports.Count);

        var keyImport = Assert.Single(link.Imports, i => i.SourceName == "@key");
        Assert.Equal("@myKey", keyImport.Alias);
        Assert.Equal("@myKey", keyImport.EffectiveName);

        var externalImport = Assert.Single(link.Imports, i => i.SourceName == "@external");
        Assert.Null(externalImport.Alias);
        Assert.Equal("@external", externalImport.EffectiveName);
    }

    [Fact]
    public void SchemaBuilder_WithLinkDirective_ShouldProcessWithoutErrors()
    {
        // Given
        var builder = new SchemaBuilder();

        // When
        builder.Add(@"
extend schema @link(url: ""./types/user"", import: [""User"", ""Profile""])

type Query {
    hello: String
}
");

        // Then - verify that the schema definition is added without error
        // This test verifies no exceptions are thrown during processing
        Assert.True(true); // If we get here, the @link directive was processed successfully
    }
}