using tanka.graphql.type;
using GraphQLParser.AST;

namespace tanka.graphql.validation
{
    public struct VariableUsage
    {
        public GraphQLVariable Node;

        public IType Type;
    }
}