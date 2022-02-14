using System.Threading.Tasks;
using Tanka.GraphQL.Extensions;
using Xunit;

namespace Tanka.GraphQL.Tests.Extensions;

public class ExtensionsImportProviderFacts
{
    private readonly ParserOptions _options;
    private readonly ExtensionsImportProvider _sut;

    public ExtensionsImportProviderFacts()
    {
        _options = new ParserOptions();
        _sut = new ExtensionsImportProvider();
    }

    [Theory]
    [InlineData("cost-analysis")]
    public void Can_Import(string extension)
    {
        /* Given */
        var path = $"tanka://{extension}";

        /* When */
        /* Then */
        Assert.True(_sut.CanImport(path, null));
    }

    [Theory]
    [InlineData("cost-analysis")]
    public async Task Import(string extension)
    {
        /* Given */
        var path = $"tanka://{extension}";

        /* When */
        var typeDefs = await _sut.ImportAsync(path, null, _options);

        /* Then */
        Assert.NotNull(typeDefs.DirectiveDefinitions);
        Assert.NotEmpty(typeDefs.DirectiveDefinitions);
    }
}