using GraphQLParser.AST;

namespace Tanka.GraphQL.Validation
{
    public delegate void NodeVisitor<in T>(T node) where T : ASTNode;
}