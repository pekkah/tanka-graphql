using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectValue : Value
    {
        public readonly IReadOnlyCollection<ObjectField> Fields;
        public readonly Location? Location;

        public ObjectValue(
            IReadOnlyCollection<ObjectField> fields,
            in Location? location = default)
        {
            Fields = fields;
            Location = location;
        }
    }
}