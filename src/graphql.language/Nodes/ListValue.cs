using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes;

public sealed class ListValue : ValueBase, ICollectionNode<ValueBase>
{
    private readonly IReadOnlyList<ValueBase> _values;

    public ListValue(
        IReadOnlyList<ValueBase> values,
        in Location? location = default)
    {
        _values = values;
        Location = location;
    }

    public override NodeKind Kind => NodeKind.ListValue;

    public override Location? Location { get; }

    public IEnumerator<ValueBase> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_values).GetEnumerator();
    }

    public int Count => _values.Count;
    public ValueBase this[int index] => _values[index];
}