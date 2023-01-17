namespace Tanka.GraphQL.Features;

public class SelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    public required ISelectionSetExecutor SelectionSetExecutor { get; set; }
}