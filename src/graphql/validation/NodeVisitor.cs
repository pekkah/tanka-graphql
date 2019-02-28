using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public delegate void NodeVisitor<in T>(T node) where T : ASTNode;
}