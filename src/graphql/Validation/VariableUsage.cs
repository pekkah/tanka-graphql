using GraphQLParser.AST;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public struct VariableUsage
    {
        public GraphQLVariable Node;

        public IType Type;

        public object DefaultValue;
    }
}