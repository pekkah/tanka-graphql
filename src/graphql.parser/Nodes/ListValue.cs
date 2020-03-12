using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListValue : Value
    {
        public readonly IReadOnlyCollection<Value> Value;
        public readonly Location? Location;

        public ListValue(
            in IReadOnlyCollection<Value> value,
            in Location? location)
        {
            Value = value;
            Location = location;
        }
    }
}