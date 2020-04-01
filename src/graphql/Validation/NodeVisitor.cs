using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Validation
{
    public delegate void NodeVisitor<in T>(T node) where T : INode;
}