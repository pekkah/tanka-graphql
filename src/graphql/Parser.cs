using System.Threading.Tasks;
using GraphQLParser;
using GraphQLParser.AST;

namespace tanka.graphql
{
    public static class Parser
    {
        /// <summary>
        ///     Parse <see cref="document"/> into <see cref="GraphQLDocument"/>
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static GraphQLDocument ParseDocument(string document)
        {
            var lexer = new Lexer();
            var parser = new GraphQLParser.Parser(lexer);
            return parser.Parse(new Source(document));
        }

        /// <summary>
        ///     Parse <see cref="document"/> into <see cref="GraphQLDocument"/>
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Task<GraphQLDocument> ParseDocumentAsync(string document)
        {
            return Task.Run(() =>
            {
                var lexer = new Lexer();
                var parser = new GraphQLParser.Parser(lexer);
                return parser.Parse(new Source(document));
            });
        }
    }
}