using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ExecutableDocument
    {
        public readonly IReadOnlyCollection<FragmentDefinition>? FragmentDefinitions;
        public readonly IReadOnlyCollection<OperationDefinition>? OperationDefinitions;

        public ExecutableDocument(
            IReadOnlyCollection<OperationDefinition>? operationDefinitions,
            IReadOnlyCollection<FragmentDefinition>? fragmentDefinitions)
        {
            OperationDefinitions = operationDefinitions;
            FragmentDefinitions = fragmentDefinitions;
        }
    }
}