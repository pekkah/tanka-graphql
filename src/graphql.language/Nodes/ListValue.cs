using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ListValue : Value
    {
        public override NodeKind Kind => NodeKind.ListValue;
        public override Location? Location {get;}
        public readonly IReadOnlyCollection<Value> Values;

        public ListValue(
            IReadOnlyCollection<Value> values,
            in Location? location = default)
        {
            Values = values;
            Location = location;
        }
    }
}