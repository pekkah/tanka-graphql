using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language
{
    public interface IReadOnlyDocumentVisitor
    {
        void EnterNode(INode node);
        void ExitNode(INode node);
    }
}