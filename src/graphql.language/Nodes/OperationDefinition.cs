﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class OperationDefinition: INode
    {
        public NodeKind Kind => NodeKind.OperationDefinition;
        public readonly IReadOnlyCollection<Directive>? Directives;
        public Location? Location {get;}
        public readonly Name? Name;
        public readonly OperationType Operation;
        public readonly SelectionSet SelectionSet;
        public readonly IReadOnlyCollection<VariableDefinition>? VariableDefinitions;

        public OperationDefinition(
            OperationType operation,
            in Name? name,
            IReadOnlyCollection<VariableDefinition>? variableDefinitions,
            IReadOnlyCollection<Directive>? directives,
            SelectionSet selectionSet,
            in Location? location = default)
        {
            Operation = operation;
            Name = name;
            VariableDefinitions = variableDefinitions;
            Directives = directives;
            SelectionSet = selectionSet;
            Location = location;
        }

        public static implicit operator OperationDefinition(string value)
        {
            var parser = new Parser(Encoding.UTF8.GetBytes(value));
            return parser.ParseOperationDefinition();
        }
    }
}