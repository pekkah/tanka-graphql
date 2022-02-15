using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Language;

public interface IReadOnlyDocumentVisitor<TContext>
{
    void EnterNode(TContext context, INode node);
    void ExitNode(TContext context, INode node);
}