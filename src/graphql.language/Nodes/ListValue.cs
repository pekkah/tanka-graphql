using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class ListValue : ValueBase, ICollectionNode<ValueBase>
{
    //todo: remove?
    public readonly IReadOnlyList<ValueBase> Values;

    public ListValue(
        IReadOnlyList<ValueBase> values,
        in Location? location = default)
    {
        Values = values;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.ListValue;

    public override Location? Location { get; }

    public IEnumerator<ValueBase> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Values).GetEnumerator();
    }

    public int Count => Values.Count;
    public ValueBase this[int index] => Values[index];
}