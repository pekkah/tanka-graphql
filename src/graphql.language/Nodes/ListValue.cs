using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListValue : IValue
    {
        public readonly Location? Location;
        public readonly IReadOnlyCollection<IValue> Value;

        public ListValue(
            IReadOnlyCollection<IValue> value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}