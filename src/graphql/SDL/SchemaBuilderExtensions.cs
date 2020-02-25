using System.Threading.Tasks;
using GraphQLParser.AST;
using Tanka.GraphQL.SchemaBuilding;

namespace Tanka.GraphQL.SDL
{
    public static class SchemaBuilderExtensions
    {
        public static SchemaBuilder Sdl(this SchemaBuilder builder, string sdl)
        {
            return Sdl(builder, Parser.ParseDocument(sdl));
        }

        public static SchemaBuilder Sdl(this SchemaBuilder builder, GraphQLDocument document)
        {
            var reader = new SdlReader(document, builder);
            reader.Read();
            return builder;
        }

        public static async Task<SchemaBuilder> SdlAsync(this SchemaBuilder builder, string sdl)
        {
            var document = await Parser.ParseDocumentAsync(sdl, ParserOptions.Sdl);
            return Sdl(builder, document);
        }
    }
}