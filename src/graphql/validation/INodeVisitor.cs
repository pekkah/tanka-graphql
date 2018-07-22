using GraphQLParser.AST;

namespace fugu.graphql.validation
{
    public interface INodeVisitor
    {
        void Enter(ASTNode node);

        void Leave(ASTNode node);
    }
}
