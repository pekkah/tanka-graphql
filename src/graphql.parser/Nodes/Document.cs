namespace Tanka.GraphQL.Language.Nodes
{
    public readonly ref struct Document
    {
        public Document(in OperationDefinition[] operationDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = default;
        }

        public readonly OperationDefinition[]? OperationDefinitions;
        public readonly FragmentDefinition[]? FragmentDefinitions;

        /*
        public readonly TypeSystemDefinition[] TypeSystemDefinitions
        public readonly TypeSystemExtension[] TypeSystemExtensions 
        */
    }

    public readonly struct OperationDefinition
    {
        public OperationDefinition(
            in OperationType operation,
            in Name? name,
            in SelectionSet selectionSet,
            in Location location
        )
        {
            Operation = operation;
            Name = name;
            Location = location;
        }

        public readonly OperationType Operation;
        public readonly Name? Name;
        public readonly Location Location;
    }

    public readonly struct SelectionSet
    {
        public SelectionSet(
            Selection[] selections,
            in Location location)
        {
            Selections = selections;
            Location = location;
        }

        private readonly Selection[] Selections;
        public readonly Location Location;
    }

    public readonly struct Selection
    {
        public Selection(
            in Location location)
        {
            Location = location;
        }

        private readonly Location Location;
    }
}