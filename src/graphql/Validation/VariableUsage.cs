using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Language.Nodes.TypeSystem;

namespace Tanka.GraphQL.Validation;

public struct VariableUsage
{
    public Variable Node;

    public TypeDefinition? Type;

    public object? DefaultValue;
}