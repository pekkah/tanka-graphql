namespace Tanka.GraphQL.Language.Nodes;

public interface ISelection : INode
{
    public Directives? Directives { get; }
    public SelectionType SelectionType { get; }
}