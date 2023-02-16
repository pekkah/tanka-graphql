namespace Tanka.GraphQL;

public interface IOperationExecutorFeature
{
    public Task Execute(QueryContext context);
}