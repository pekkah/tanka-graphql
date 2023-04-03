using System;
using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public abstract class DocumentWalkerContextBase
{
    private readonly Stack<ArrayState> _arrayStates = new();
    private readonly Stack<INode> _nodes = new();
    private readonly Stack<INode> _parents = new();

    public INode Current => _nodes.Peek();

    public ArrayState? CurrentArray => _arrayStates.TryPeek(out var state) ? state : null;

    public IEnumerable<INode> Nodes => _nodes;

    public INode? Parent => _parents.Count > 0 ? _parents.Peek() : null;

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

    public bool Contains(INode node)
    {
        return _nodes.Contains(node);
    }
}

public sealed class ArrayState
{
    public ArrayState(ICollectionNode<INode> array)
    {
        Array = array;
    }

    public ICollectionNode<INode> Array { get; }

    public int Count => Array.Count;

    public INode CurrentItem => Array[Index];

    public int Index { get; set; }


    public bool IsFirst => Index == 0;

    public bool IsLast => Index == Count - 1;
}