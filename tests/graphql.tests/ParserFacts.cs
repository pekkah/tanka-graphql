using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser.AST;
using NSubstitute;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests
{
    public class ParserFacts
    {
        [Fact]
        public async Task Use_import_provider()
        {
            /* Given */
            var provider = Substitute.For<IDocumentImportProvider>();
            provider.CanImport(null).ReturnsForAnyArgs(true);
            provider.ImportAsync(null).ReturnsForAnyArgs(
                new List<ASTNode>()
                {
                    new GraphQLObjectTypeDefinition()
                    {
                        Name = new GraphQLName()
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
            var document = await Parser.ParseDocumentAsync(
                sdl,
                new ParserOptions
                {
                    ImportProviders = new List<IDocumentImportProvider>
                    {
                        provider
                    }
                });

            /* Then */
            Assert.Single(
                document.Definitions.OfType<GraphQLObjectTypeDefinition>(),
                objectTypeDef => objectTypeDef.Name.Value == "Imported");
        }
    }
}