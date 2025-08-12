using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.TypeSystem;

using Xunit;

namespace Tanka.GraphQL.Tests.TypeSystem;

public class SchemaLoaderTests
{
    [Fact]
    public void FileSchemaLoader_CanLoad_ShouldReturnTrueForFilePaths()
    {
        // Given
        var loader = new FileSchemaLoader();

        // Then
        Assert.True(loader.CanLoad("file://test.graphql"));
        Assert.True(loader.CanLoad("./test.graphql"));
        Assert.True(loader.CanLoad("../test.graphql"));
        Assert.True(loader.CanLoad("test.graphql"));
        Assert.False(loader.CanLoad("http://example.com/test.graphql"));
        Assert.False(loader.CanLoad("https://example.com/test.graphql"));
    }

    [Fact]
    public async Task FileSchemaLoader_LoadSchemaAsync_ShouldLoadValidFile()
    {
        // Given
        var tempFile = Path.GetTempFileName();
        var schema = @"
type Query {
    hello: String
}";
        await File.WriteAllTextAsync(tempFile, schema);

        try
        {
            var loader = new FileSchemaLoader();

            // When
            var result = await loader.LoadSchemaAsync(tempFile);

            // Then
            Assert.NotNull(result);
            Assert.NotNull(result.TypeDefinitions);
            Assert.Single(result.TypeDefinitions);
            Assert.Equal("Query", result.TypeDefinitions[0].Name.Value);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task FileSchemaLoader_LoadSchemaAsync_ShouldReturnNullForNonExistentFile()
    {
        // Given
        var loader = new FileSchemaLoader();

        // When
        var result = await loader.LoadSchemaAsync("./non-existent-file.graphql");

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task FileSchemaLoader_LoadSchemaAsync_WithBasePath_ShouldResolveRelativePaths()
    {
        // Given
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var schemaFile = Path.Combine(tempDir, "schema.graphql");
        var schema = @"
type User {
    id: ID!
}";
        await File.WriteAllTextAsync(schemaFile, schema);

        try
        {
            var loader = new FileSchemaLoader(tempDir);

            // When
            var result = await loader.LoadSchemaAsync("schema.graphql");

            // Then
            Assert.NotNull(result);
            Assert.NotNull(result.TypeDefinitions);
            Assert.Single(result.TypeDefinitions);
            Assert.Equal("User", result.TypeDefinitions[0].Name.Value);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void HttpSchemaLoader_CanLoad_ShouldReturnTrueForHttpUrls()
    {
        // Given
        var loader = new HttpSchemaLoader();

        // Then
        Assert.True(loader.CanLoad("http://example.com/schema.graphql"));
        Assert.True(loader.CanLoad("https://example.com/schema.graphql"));
        Assert.True(loader.CanLoad("HTTP://EXAMPLE.COM/SCHEMA.GRAPHQL"));
        Assert.True(loader.CanLoad("HTTPS://EXAMPLE.COM/SCHEMA.GRAPHQL"));
        Assert.False(loader.CanLoad("file://test.graphql"));
        Assert.False(loader.CanLoad("./test.graphql"));
    }

    [Fact]
    public void CompositeSchemaLoader_CanLoad_ShouldCheckAllLoaders()
    {
        // Given
        var httpLoader = Substitute.For<ISchemaLoader>();
        httpLoader.CanLoad("http://example.com/schema.graphql").Returns(true);
        httpLoader.CanLoad("file://test.graphql").Returns(false);

        var fileLoader = Substitute.For<ISchemaLoader>();
        fileLoader.CanLoad("file://test.graphql").Returns(true);
        fileLoader.CanLoad("http://example.com/schema.graphql").Returns(false);

        var compositeLoader = new CompositeSchemaLoader(httpLoader, fileLoader);

        // Then
        Assert.True(compositeLoader.CanLoad("http://example.com/schema.graphql"));
        Assert.True(compositeLoader.CanLoad("file://test.graphql"));
        Assert.False(compositeLoader.CanLoad("unknown://test.graphql"));
    }

    [Fact]
    public async Task CompositeSchemaLoader_LoadSchemaAsync_ShouldTryLoadersInOrder()
    {
        // Given
        var schema = Parser.Create("type Query { field: String }").ParseTypeSystemDocument();

        var httpLoader = Substitute.For<ISchemaLoader>();
        httpLoader.CanLoad("http://example.com/schema.graphql").Returns(true);
        httpLoader.LoadSchemaAsync("http://example.com/schema.graphql", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Language.Nodes.TypeSystem.TypeSystemDocument?>(schema));

        var fileLoader = Substitute.For<ISchemaLoader>();
        fileLoader.CanLoad("http://example.com/schema.graphql").Returns(false);

        var compositeLoader = new CompositeSchemaLoader(httpLoader, fileLoader);

        // When
        var result = await compositeLoader.LoadSchemaAsync("http://example.com/schema.graphql");

        // Then
        Assert.Same(schema, result);
        await httpLoader.Received(1).LoadSchemaAsync("http://example.com/schema.graphql", Arg.Any<CancellationToken>());
        await fileLoader.DidNotReceive().LoadSchemaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CompositeSchemaLoader_CreateDefault_ShouldCreateWithHttpAndFileLoaders()
    {
        // When
        var loader = CompositeSchemaLoader.CreateDefault();

        // Then
        Assert.True(loader.CanLoad("http://example.com/schema.graphql"));
        Assert.True(loader.CanLoad("https://example.com/schema.graphql"));
        Assert.True(loader.CanLoad("file://test.graphql"));
        Assert.True(loader.CanLoad("./test.graphql"));
    }
}