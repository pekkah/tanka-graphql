using System.Collections;
using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public abstract class CollectionNodeBase<T> : ICollectionNode<T> where T : INode
    {
        private readonly IReadOnlyCollection<T> _items;

        protected CollectionNodeBase(
            IReadOnlyCollection<T> items,
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
    }
}