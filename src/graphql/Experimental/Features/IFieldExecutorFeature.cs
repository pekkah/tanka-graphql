namespace Tanka.GraphQL.Experimental.Features;

public interface IFieldExecutorFeature
{
    IFieldExecutor FieldExecutor { get; set; }
}