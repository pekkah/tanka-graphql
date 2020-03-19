using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class OperationDefinition
    {
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly Location? Location;
        public readonly Name? Name;
        public readonly OperationType Operation;
        public readonly SelectionSet SelectionSet;
        public readonly IReadOnlyCollection<VariableDefinition>? VariableDefinitions;

        public OperationDefinition(
            OperationType operation,
            Name? name,
            IReadOnlyCollection<VariableDefinition>? variableDefinitions,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet selectionSet,
            in Location? location)
        {
            Operation = operation;
            Name = name;
            VariableDefinitions = variableDefinitions;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }
    }
}