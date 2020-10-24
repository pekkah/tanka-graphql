using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class Import : INode
    {
        public Import(
            IReadOnlyList<Name>? types,
            StringValue from,
            in Location? location)
        {
            Types = types;
            From = from;
            Location = location;
        }

        public StringValue From { get; }

        public NodeKind Kind => NodeKind.TankaImport;
        public Location? Location { get; }
        public IReadOnlyList<Name>? Types { get; }
    }
}