namespace Tanka.GraphQL.Experimental.Features;

public interface ISelectionSetExecutorFeature
{
    ISelectionSetExecutor SelectionSetExecutor { get; set; }
}