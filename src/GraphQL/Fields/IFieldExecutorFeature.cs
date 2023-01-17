namespace Tanka.GraphQL.Fields;

public interface IFieldExecutorFeature
{
    IFieldExecutor FieldExecutor { get; set; }
}