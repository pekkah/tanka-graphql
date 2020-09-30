using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public interface ICollectionNode<out T> : INode, IReadOnlyCollection<T> where T : INode
    {
    }
}