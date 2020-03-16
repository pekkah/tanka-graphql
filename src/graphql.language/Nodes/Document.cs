using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class Document
    {
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;
        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public Document(
            IReadOnlyCollection<OperationDefinition>? operationDefinitions,
            IReadOnlyCollection<FragmentDefinition>? fragmentDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = fragmentDefinitions;
        }
    }
}