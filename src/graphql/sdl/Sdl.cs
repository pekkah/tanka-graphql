using System;
using GraphQLParser.AST;
using tanka.graphql.schema;
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

        public static void Import(GraphQLDocument document, SchemaBuilder builder)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            var reader = new SdlReader(document, builder);
            reader.Read();
        }
    }
}