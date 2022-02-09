using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation;

public struct VariableUsage
{
    public Variable Node;

    public TypeBase Type;

    public object DefaultValue;
}