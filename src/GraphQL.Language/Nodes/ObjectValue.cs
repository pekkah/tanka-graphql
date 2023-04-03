using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class ObjectValue : ValueBase, ICollectionNode<ObjectField>
{
    //todo: remove?
    public readonly IReadOnlyList<ObjectField> Fields;

    public ObjectValue(
        IReadOnlyList<ObjectField> fields,
        in Location? location = default)
    {
        Fields = fields;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.ObjectValue;

    public override Location? Location { get; }

    public IEnumerator<ObjectField> GetEnumerator()
    {
        return Fields.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Fields).GetEnumerator();
    }

    public int Count => Fields.Count;
    public ObjectField this[int index] => Fields[index];
}