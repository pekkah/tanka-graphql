using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using Xunit;

namespace Tanka.GraphQL.Tests
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
            var import = Parser.ImportDirectiveType.CreateInstance(
                new Dictionary<string, object>
                {
                    ["path"] = importPath
                });


            /* When */
            /* Then */
            Assert.True(_sut.CanImport(import));
        }

        [Fact]
        public async Task Import()
        {
            /* Given */
            var importPath = $"embedded://tanka.graphql.tests/{_embeddedResourceName}";
            var import = Parser.ImportDirectiveType.CreateInstance(
                new Dictionary<string, object>
                {
                    ["path"] = importPath
                });


            /* When */
            var typeDefs = await _sut.ImportAsync(import, _options);

            /* Then */
            Assert.Single(
                typeDefs.OfType<GraphQLDirectiveDefinition>(),
                dt => dt.Name.Value == "Directive");
        }
    }
}