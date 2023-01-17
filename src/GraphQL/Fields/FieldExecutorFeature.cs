namespace Tanka.GraphQL.Fields;

public class FieldExecutorFeature : IFieldExecutorFeature
{
    public required IFieldExecutor FieldExecutor { get; set; }
}