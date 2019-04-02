using GraphQLParser.AST;

namespace tanka.graphql.language
{
    public static class AstNodeExtensions
    {
        public static string ToGraphQL(this ASTNode node)
        {
            return new Printer().Print(node);
        }
    }
}