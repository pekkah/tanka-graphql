using System.Threading.Tasks;
using Tanka.GraphQL.Language.ImportProviders;
using Xunit;

namespace Tanka.GraphQL.Tests.Language.ImportProviders;

public class FileSystemImportProviderFacts
{
    private readonly string _importedFileName;
    private readonly ParserOptions _options;
    private readonly FileSystemImportProvider _sut;

    public FileSystemImportProviderFacts()
    {
        _options = new ParserOptions();
        _sut = new FileSystemImportProvider();
        _options.ImportProviders.Add(_sut);
        _importedFileName = "Files/Import.graphql";
    }

    [Fact]
    public void Can_Import()
    {
        /* Given */
        var path = _importedFileName;

        /* When */
        /* Then */
        Assert.True(_sut.CanImport(path, null));
    }

    [Fact]
    public async Task Import()
    {
        /* Given */
        var path = _importedFileName;

        /* When */
        var typeDefs = await _sut.ImportAsync(path, null, _options);

        /* Then */
        Assert.NotNull(typeDefs.DirectiveDefinitions);
        Assert.Single(
            typeDefs.DirectiveDefinitions,
            dt => dt.Name == "directive");
    }
}