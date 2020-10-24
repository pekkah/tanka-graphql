using System.Collections.Generic;

namespace Tanka.GraphQL.Language.Nodes
{
    public interface ICollectionNode<out T> : INode, IReadOnlyList<T> where T : INode
    {
    }
}