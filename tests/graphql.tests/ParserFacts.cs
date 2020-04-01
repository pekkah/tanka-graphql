using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NSubstitute;
using Tanka.GraphQL.Language;
using Xunit;

namespace Tanka.GraphQL.Tests
{
    public class ParserFacts
    {
        [Fact]
        public async Task Use_import_provider()
        {
            /* Given */
            var provider = Substitute.For<IImportProvider>();
            provider.CanImport(null, null).ReturnsForAnyArgs(true);
            provider.ImportAsync(null, null, null).ReturnsForAnyArgs(
                new List<object>
                {
                    new ObjectDefinition
                    {
                        Name = new Name
                        {
                            Value = "Imported"
                        }
                    }
                });

            var sdl = @"
                # @import(path: ""./Imported"")

                type Query {
                    field: Imported
                }
                ";


            /* When */
            var document = await Parser.ParseTypeSystemDocumentAsync(
                sdl,
                new ParserOptions
                {
                    ImportProviders = new List<IImportProvider>
                    {
                        provider
                    }
                });

            /* Then */
            Assert.Single(
                document.Definitions.OfType<ObjectDefinition>(),
                objectTypeDef => objectTypeDef.Name.Value == "Imported");
        }
    }
}