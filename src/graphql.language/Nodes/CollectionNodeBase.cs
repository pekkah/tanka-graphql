using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class CollectionNodeBase<T> : ICollectionNode<T> where T : INode
    {
        private readonly IReadOnlyList<T> _items;

        protected CollectionNodeBase(
            IReadOnlyList<T> items,
            in Location? location = default)
        {
            _items = items;
            Location = location;
        }

        public abstract NodeKind Kind { get; }

        public Location? Location { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public int Count => _items.Count;

        public T this[int index] => _items[index];
    }
}