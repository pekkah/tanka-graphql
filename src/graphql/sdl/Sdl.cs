using GraphQLParser.AST;
using tanka.graphql.type;

namespace tanka.graphql.sdl
{
    public static class Sdl
    {
        public static ISchema Schema(GraphQLDocument document, SchemaBuilder builder = null)
        {
            var reader = new SdlReader(document, builder);
            return reader.Read().Build();
        }
    }
}