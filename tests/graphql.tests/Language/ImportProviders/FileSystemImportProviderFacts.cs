using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.ImportProviders;
using Xunit;

namespace Tanka.GraphQL.Tests.Language.ImportProviders
{
    public class FileSystemImportProviderFacts
    {
        private ParserOptions _options;
        private FileSystemImportProvider _sut;
        private string _importedFileName;

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
            Assert.Single(
                typeDefs.OfType<DirectiveDefinition>(),
                dt => dt.Name.Value == "directive");
        }
    }
}