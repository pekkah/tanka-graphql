using System.Threading.Tasks;
using Tanka.GraphQL.TypeSystem;
using Xunit;

namespace Tanka.GraphQL.Tests.Language.ImportProviders
{
    public class FileSystemImportFacts
    {
        [Fact]
        public async Task Parse_Sdl()
        {
            /* Given */
            var sdl =
                  @"
                    """"""
                    tanka_import from ""Files/Import""
                    """"""

                    type Query {
                        imported: ImportedType
                    }
                 ";

            /* When */
            var builder = new SchemaBuilder()
                // BuiltIn import providers are used
                .Add(sdl);

            var schema = await builder.Build(new SchemaBuildOptions());

            /* Then */
            var importedType = schema.GetNamedType("ImportedType");
            Assert.NotNull(importedType);
           
            var importedField = schema.GetField(schema.Query.Name, "imported");
            Assert.Equal(importedType.Name, importedField.Type.Unwrap().Name);
            
            var nestedType = schema.GetNamedType("NestedObject");
            Assert.NotNull(nestedType);
        }
    }
}