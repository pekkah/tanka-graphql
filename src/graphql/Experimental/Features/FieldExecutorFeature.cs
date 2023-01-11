namespace Tanka.GraphQL.Experimental.Features;

public class FieldExecutorFeature : IFieldExecutorFeature
{
    public required IFieldExecutor FieldExecutor { get; set; }
}