using System.Text;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class OperationDefinition : INode
{
    public readonly Directives? Directives;
    public readonly bool IsShort;
    public readonly Name? Name;
    public readonly OperationType Operation;
    public readonly SelectionSet SelectionSet;
    public readonly VariableDefinitions? VariableDefinitions;

    public OperationDefinition(
        OperationType operation,
        in Name? name,
        VariableDefinitions? variableDefinitions,
        Directives? directives,
        SelectionSet selectionSet,
        in Location? location = default,
        bool isShort = false)
    {
        Operation = operation;
        Name = name;
        VariableDefinitions = variableDefinitions;
        Directives = directives;
        SelectionSet = selectionSet;
        Location = location;
        IsShort = isShort;
    }

    public NodeKind Kind => NodeKind.OperationDefinition;
    public Location? Location { get; }

    public static implicit operator OperationDefinition(string value)
    {
        var parser = Parser.Create(value);
        return parser.ParseOperationDefinition();
    }
}