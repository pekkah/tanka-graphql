using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class Document
    {
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;

        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public Document(
            in IReadOnlyCollection<OperationDefinition> operationDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = default;
        }

        /*
        public readonly TypeSystemDefinition[] TypeSystemDefinitions
        public readonly TypeSystemExtension[] TypeSystemExtensions 
        */
    }

    public class OperationDefinition
    {
        public readonly Location Location;
        public readonly Name? Name;
        public readonly OperationType Operation;
        public readonly SelectionSet SelectionSet;

        public OperationDefinition(
            in OperationType operation,
            in Name? name,
            in SelectionSet selectionSet,
            in Location location
        )
        {
            Operation = operation;
            Name = name;
            SelectionSet = selectionSet;
            Location = location;
        }
    }

    public class SelectionSet
    {
        public readonly Location Location;
        public readonly IReadOnlyCollection<ISelection> Selections;

        public SelectionSet(
            in IReadOnlyCollection<ISelection> selections,
            in Location location)
        {
            Selections = selections;
            Location = location;
        }
    }

    public class FieldSelection : ISelection
    {
        public readonly Location Location;
        public readonly Name? Alias;
        public readonly Name Name;
        public readonly SelectionSet? SelectionSet;

        public FieldSelection(
            in Name? alias,
            in Name name,
            in SelectionSet? selectionSet,
            in Location location)
        {
            Alias = alias;
            Name = name;
            SelectionSet = selectionSet;
            Location = location;
        }

        public SelectionType SelectionType => SelectionType.Field;
    }
}