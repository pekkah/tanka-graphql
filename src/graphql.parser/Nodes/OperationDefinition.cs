﻿using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class OperationDefinition
    {
        public readonly Location Location;
        public readonly Name? Name;
        public readonly OperationType Operation;
        public readonly IReadOnlyCollection<VariableDefinition>? VariableDefinitions;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public readonly SelectionSet SelectionSet;

        public OperationDefinition(
            in OperationType operation,
            in Name? name,
            in IReadOnlyCollection<VariableDefinition>? variableDefinitions,
            in IReadOnlyCollection<Directive>? directives,
            in SelectionSet selectionSet,
            in Location location
        )
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