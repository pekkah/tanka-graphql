namespace Tanka.GraphQL.Features;

public interface ISelectionSetExecutorFeature
{
    ISelectionSetExecutor SelectionSetExecutor { get; set; }
}