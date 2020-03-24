using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListValue : Value
    {
        public readonly Location? Location;
        public readonly IReadOnlyCollection<Value> Value;

        public ListValue(
            IReadOnlyCollection<Value> value,
            in Location? location = default)
        {
            Value = value;
            Location = location;
        }
    }
}