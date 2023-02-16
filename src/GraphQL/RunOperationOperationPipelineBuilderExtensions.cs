namespace Tanka.GraphQL;

public static class RunOperationOperationPipelineBuilderExtensions
{
    public static OperationPipelineBuilder RunOperation(this OperationPipelineBuilder builder)
    {
        builder.Use(_ => context => context.ExecuteOperation());

        return builder;
    }
}