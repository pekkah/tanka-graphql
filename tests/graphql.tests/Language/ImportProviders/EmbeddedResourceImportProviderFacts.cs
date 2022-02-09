using System.Threading.Tasks;
using Tanka.GraphQL.Language.ImportProviders;
using Xunit;

namespace Tanka.GraphQL.Tests.Language.ImportProviders;

public class EmbeddedResourceImportProviderFacts
{
    private readonly string _embeddedResourceName;
    private readonly ParserOptions _options;

    private readonly EmbeddedResourceImportProvider _sut;

    public EmbeddedResourceImportProviderFacts()
    {
        _options = ParserOptions.Sdl;
        _sut = new EmbeddedResourceImportProvider();
        _embeddedResourceName = "Tanka.GraphQL.Tests.Files.Embedded.graphql";
    }

    [Fact]
    public void Can_import()
    {
        /* Given */
        var importPath = $"embedded://tanka.graphql.tests/{_embeddedResourceName}";

        /* When */
        /* Then */
        Assert.True(_sut.CanImport(importPath, null));
    }

    [Fact]
    public async Task Import()
    {
        /* Given */
        var importPath = $"embedded://tanka.graphql.tests/{_embeddedResourceName}";

        /* When */
        var typeDefs = await _sut.ImportAsync(importPath, null, _options);

        /* Then */
        Assert.NotNull(typeDefs.DirectiveDefinitions);
        Assert.Single(
            typeDefs.DirectiveDefinitions,
            dt => dt.Name == "directive");
    }
}