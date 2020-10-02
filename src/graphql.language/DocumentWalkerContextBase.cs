using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public abstract class DocumentWalkerContextBase
    {
        private readonly Stack<INode> _nodes  = new Stack<INode>();
        private readonly Stack<INode> _parents = new Stack<INode>();
        private readonly Stack<ArrayState> _arrayStates = new Stack<ArrayState>();

        public IEnumerable<INode> Nodes => _nodes;

        public INode Current => _nodes.Peek();

        public INode? Parent => _parents.Count > 0 ? _parents.Peek(): null;

        public ArrayState? CurrentArray => _arrayStates.TryPeek(out var state) ? state : null;

        public void Push(INode node)
        {
            if (_nodes.Count > 0)
                _parents.Push(_nodes.Peek());

            _nodes.Push(node);
        }

        public ArrayState PushArrayState(ICollectionNode<INode> array)
        {
            var state = new ArrayState(array);
            _arrayStates.Push(state);
            return state;
        }

        public ArrayState? PopArrayState()
        {
            _arrayStates.TryPop(out var state);
            return state;
        }

        public INode Pop()
        {
            var node = _nodes.Pop();
            _parents.TryPop(out _);
            return node;
        }

        public bool Contains(INode node) => _nodes.Contains(node);
    }

    public sealed class ArrayState
    {
        public ICollectionNode<INode> Array { get; }

        public ArrayState(ICollectionNode<INode> array)
        {
            Array = array;
        }

        public int Count => Array.Count;

        public int Index { get; set; }

        public INode CurrentItem => Array[Index];


        public bool IsFirst => Index == 0;

        public bool IsLast => Index == Count - 1;
    }
}