using GraphQLParser.AST;

namespace Tanka.GraphQL.Language
{
    public static class AstNodeExtensions
    {
        public static string ToGraphQL(this ASTNode node)
        {
            return new Printer().Print(node);
        }
    }
}