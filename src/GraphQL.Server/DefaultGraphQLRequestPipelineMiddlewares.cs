namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLRequestPipelineMiddlewares
{
    public static GraphQLRequestPipelineBuilder RunOperationExecutor(
        this GraphQLRequestPipelineBuilder builder,
        Action<OperationDelegateBuilder> configureOperation)
    {
        var operationDelegateBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        configureOperation(operationDelegateBuilder);
        OperationDelegate operationDelegate = operationDelegateBuilder.Build();

        var executor = new Executor(operationDelegate);
        return builder.Use(_ => context => executor.ExecuteContext(context));
    }

    public static GraphQLRequestPipelineBuilder UseDefaults(
        this GraphQLRequestPipelineBuilder builder,
        string schemaName)
    {
        return builder
            .UseSchema(schemaName)
            .RunOperationExecutor(operation =>
            {
                //todo: for now use this to pass the trace enabled allowing setting it up without modifying whole pipeline
                operation.SetProperty("TraceEnabled", builder.GetProperty<bool>("TraceEnabled"));
                operation.UseDefaults();
            });
    }
}