using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes.TypeSystem
{
    public sealed class Import
    {
        public Import(
            IReadOnlyCollection<Name>? types,
            StringValue from,
            in Location? location)
        {
            Types = types;
            From = from;
            Location = location;
        }

        public StringValue From { get; }

        public Location? Location { get; }
        public IReadOnlyCollection<Name>? Types { get; }
    }
}