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
}