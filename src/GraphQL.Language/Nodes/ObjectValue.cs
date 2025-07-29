using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class ObjectValue(
    IReadOnlyList<ObjectField> fields,
    in Location? location = default)
    : ValueBase, ICollectionNode<ObjectField>
{
    public override NodeKind Kind => NodeKind.ObjectValue;

    public override Location? Location { get; } = location;

    public IEnumerator<ObjectField> GetEnumerator()
    {
        return fields.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)fields).GetEnumerator();
    }

    public int Count => fields.Count;

    public ObjectField this[int index] => fields[index];
}