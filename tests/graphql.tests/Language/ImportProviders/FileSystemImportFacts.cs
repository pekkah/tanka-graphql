using System.Threading.Tasks;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
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
                  @"tanka_import from ""Files/Import""

                    type Query {
                        imported: ImportedType
                    }
                 ";

            /* When */
            var builder = await new SchemaBuilder()
                // BuiltIn import providers are used
                .SdlAsync(sdl);

            var schema = builder.Build();

            /* Then */
            var importedType = schema.GetNamedType<ObjectType>("ImportedType");
            Assert.NotNull(importedType);
            var importedField = schema.GetField(schema.Query.Name, "imported");
            Assert.Same(importedType, importedField.Type.Unwrap());
            var nestedType = schema.GetNamedType<ObjectType>("NestedObject");
            Assert.NotNull(nestedType);
        }
    }
}