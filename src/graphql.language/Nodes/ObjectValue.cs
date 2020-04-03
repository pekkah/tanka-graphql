using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public sealed class ObjectValue : Value, INode
    {
        public override NodeKind Kind => NodeKind.ObjectValue;
        public readonly IReadOnlyCollection<ObjectField> Fields;
        public override Location? Location {get;}

        public ObjectValue(
            IReadOnlyCollection<ObjectField> fields,
            in Location? location = default)
        {
            Fields = fields;
            Location = location;
        }
    }
}