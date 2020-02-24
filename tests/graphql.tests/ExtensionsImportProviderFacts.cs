using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.GraphQL.Extensions;
using Xunit;

namespace Tanka.GraphQL.Tests
{
    public class ExtensionsImportProviderFacts
    {
        private readonly ExtensionsImportProvider _sut;
        private ParserOptions _options;

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
            var import = Parser.ImportDirectiveType.CreateInstance(
                new Dictionary<string, object>
                {
                    ["path"] = $"tanka://{extension}"
                });

            /* When */
            /* Then */
            Assert.True(_sut.CanImport(import));
        }

        [Theory]
        [InlineData("cost-analysis")]
        public async Task Import(string extension)
        {
            /* Given */
            var import = Parser.ImportDirectiveType.CreateInstance(
                new Dictionary<string, object>
                {
                    ["path"] = $"tanka://{extension}"
                });

            /* When */
            var typeDefs = await _sut.ImportAsync(import, _options);

            /* Then */
            Assert.NotEmpty(typeDefs);
        }
    }
}