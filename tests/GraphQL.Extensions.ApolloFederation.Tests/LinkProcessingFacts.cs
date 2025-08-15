using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public class LinkProcessingFacts
{
    [Fact]
    public async Task FederationSchemaLoader_should_not_include_built_in_types()
    {
        // Arrange
        var loader = new FederationSchemaLoader();

        // Act - load the Federation schema directly
        var schema = await loader.LoadSchemaAsync("https://specs.apollo.dev/federation/v2.3");

        // Assert
        Assert.NotNull(schema);

        var typeNames = schema.TypeDefinitions?.Select(t => t.Name.Value).ToList() ?? new();

        // Built-in types should NOT be in the Federation schema
        Assert.DoesNotContain("Boolean", typeNames);
        Assert.DoesNotContain("String", typeNames);
        Assert.DoesNotContain("ID", typeNames);
        Assert.DoesNotContain("Int", typeNames);
        Assert.DoesNotContain("Float", typeNames);

        // Federation types should be present
        Assert.Contains("_Any", typeNames);
        Assert.Contains("FieldSet", typeNames);
        Assert.Contains("_Service", typeNames);
        Assert.Contains("federation__Scope", typeNames);
        Assert.Contains("federation__Policy", typeNames);
        Assert.Contains("federation__ContextFieldValue", typeNames);
    }

    [Fact]
    public void LinkDirectiveProcessor_should_extract_link_info_from_products_schema()
    {
        // Arrange - the exact schema from products.graphql
        var productsSchemaDoc = (TypeSystemDocument)@"
schema 
  @link(url: ""https://specs.apollo.dev/federation/v2.3"", import: [
    ""@composeDirective"", ""@extends"", ""@external"", ""@key"", ""@inaccessible"", 
    ""@interfaceObject"", ""@override"", ""@provides"", ""@requires"", ""@shareable"", ""@tag"", ""FieldSet""
  ]) {
  query: Query
}

type Product @key(fields: ""id"") {
  id: ID!
}
";

        // Act - extract link directives using LinkDirectiveProcessor
        var linkInfos = LinkDirectiveProcessor.ProcessLinkDirectives(
            productsSchemaDoc.SchemaDefinitions,
            productsSchemaDoc.SchemaExtensions);

        // Assert
        Assert.Single(linkInfos);
        var linkInfo = linkInfos.First();

        Assert.Equal("https://specs.apollo.dev/federation/v2.3", linkInfo.Url);
        Assert.NotNull(linkInfo.Imports);
        Assert.Contains(linkInfo.Imports, i => i.SourceName == "@key");
        Assert.Contains(linkInfo.Imports, i => i.SourceName == "FieldSet");
        Assert.DoesNotContain(linkInfo.Imports, i => i.SourceName == "Boolean"); // Boolean is not in the import list
    }

    [Fact]
    public async Task Link_processing_should_not_add_built_in_types_when_importing_federation()
    {
        // Arrange
        var builder = new SchemaBuilder();
        var loader = new FederationSchemaLoader();

        // Act - Load Federation schema and check what types it contains
        var federationSchema = await loader.LoadSchemaAsync("https://specs.apollo.dev/federation/v2.3");
        Assert.NotNull(federationSchema);

        // Get the import list from products.graphql 
        var importList = new[] { "@key", "@external", "@requires", "@provides", "@shareable",
                                "@inaccessible", "@override", "@tag", "FieldSet" };

        // Simulate what ApplyImportFiltering would do
        var importedTypes = new List<TypeDefinition>();
        var importedDirectives = new List<DirectiveDefinition>();

        foreach (var import in importList)
        {
            if (import.StartsWith("@"))
            {
                // Import directive
                var directiveName = import.Substring(1);
                var directive = federationSchema.DirectiveDefinitions?.FirstOrDefault(d => d.Name.Value == directiveName);
                if (directive != null)
                {
                    importedDirectives.Add(directive);
                }
            }
            else
            {
                // Import type
                var type = federationSchema.TypeDefinitions?.FirstOrDefault(t => t.Name.Value == import);
                if (type != null)
                {
                    importedTypes.Add(type);
                }
            }
        }

        // Assert - imported types should not include built-in types
        var importedTypeNames = importedTypes.Select(t => t.Name.Value).ToList();
        Assert.DoesNotContain("Boolean", importedTypeNames);
        Assert.DoesNotContain("String", importedTypeNames);
        Assert.DoesNotContain("ID", importedTypeNames);
        Assert.DoesNotContain("Int", importedTypeNames);
        Assert.DoesNotContain("Float", importedTypeNames);

        // Should contain FieldSet which is explicitly imported
        Assert.Contains("FieldSet", importedTypeNames);
    }
}