using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Language.ImportProviders;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Language.ImportProviders
{
    public class EmbeddedResourceImportProviderFacts
    {
        public EmbeddedResourceImportProviderFacts()
        {
            _options = new ParserOptions();
            _sut = new EmbeddedResourceImportProvider();
            _embeddedResourceName = "Tanka.GraphQL.Tests.Files.Import.graphql";
        }

        private readonly EmbeddedResourceImportProvider _sut;
        private readonly string _embeddedResourceName;
        private readonly ParserOptions _options;

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
            Assert.Single(
                typeDefs.OfType<DirectiveDefinition>(),
                dt => dt.Name == "directive");
        }
    }
}