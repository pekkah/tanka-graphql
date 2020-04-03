
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Validation
{
    public struct VariableUsage
    {
        public Variable Node;

        public IType Type;

        public object DefaultValue;
    }
}