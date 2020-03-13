using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public class Document
    {
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;
        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public Document(
            in IReadOnlyCollection<OperationDefinition>? operationDefinitions,
            in IReadOnlyCollection<FragmentDefinition>? fragmentDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = fragmentDefinitions;
        }
    }
}