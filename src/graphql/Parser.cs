using GraphQLParser;
using GraphQLParser.AST;

namespace fugu.graphql
{
    public static class Parser
    {
        public static GraphQLDocument ParseDocument(string document)
        {
            var lexer = new Lexer();
            var parser = new GraphQLParser.Parser(lexer);
            return parser.Parse(new Source(document));
        }
    }
}