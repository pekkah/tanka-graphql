using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectValue : IValue
    {
        public readonly IReadOnlyCollection<ObjectField> Fields;
        public readonly Location Location;

        public ObjectValue(
            in IReadOnlyCollection<ObjectField> fields,
            in Location location)
        {
            Fields = fields;
            Location = location;
        }
    }
}