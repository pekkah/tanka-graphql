namespace Tanka.GraphQL.Experimental.Features;

public class SelectionSetExecutorFeature : ISelectionSetExecutorFeature
{
    public required ISelectionSetExecutor SelectionSetExecutor { get; set; }
}