using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListValue : IValue
    {
        public readonly IReadOnlyCollection<IValue> Value;
        public readonly Location Location;

        public ListValue(
            in IReadOnlyCollection<IValue> value,
            in Location location)
        {
            Value = value;
            Location = location;
        }
    }
}