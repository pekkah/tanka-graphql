using System.Threading.Tasks;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.SDL
{
    public static class SchemaBuilderExtensions
    {
        public static SchemaBuilder Sdl(this SchemaBuilder builder, string sdl)
        {
            return Sdl(builder, Parser.ParseTypeSystemDocument(sdl));
        }

        public static SchemaBuilder Sdl(this SchemaBuilder builder, TypeSystemDocument document)
        {
            var reader = new SdlReader(document, builder);
            reader.Read();
            return builder;
        }

        public static async Task<SchemaBuilder> SdlAsync(this SchemaBuilder builder, string sdl)
        {
            var document = await Parser.ParseTypeSystemDocumentAsync(sdl, ParserOptions.Sdl);
            return Sdl(builder, document);
        }
    }
}