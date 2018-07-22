using fugu.graphql.type;
using GraphQLParser.AST;

namespace fugu.graphql.validation
{
    public struct VariableUsage
    {
        public GraphQLVariable Node;

        public IGraphQLType Type;
    }
}