using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.sdl
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
    }
}