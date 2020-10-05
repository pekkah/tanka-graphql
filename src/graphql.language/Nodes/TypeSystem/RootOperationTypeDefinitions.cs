using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class RootOperationTypeDefinitions: CollectionNodeBase<RootOperationTypeDefinition>
    {
        public RootOperationTypeDefinitions(IReadOnlyList<RootOperationTypeDefinition> items, in Location? location = default) : base(items, in location)
        {
        }

        public override NodeKind Kind => NodeKind.RootOperationTypeDefinitions;
    }
}