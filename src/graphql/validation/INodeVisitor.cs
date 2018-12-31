using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public interface INodeVisitor
    {
        void Enter(ASTNode node);

        void Leave(ASTNode node);
    }
}
