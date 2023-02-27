using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLRequestPipelineMiddlewares
{
    public static GraphQLRequestPipelineBuilder RunOperationExecutor(
        this GraphQLRequestPipelineBuilder builder,
        Action<OperationPipelineBuilder> configureOperation)
    {
        var operationDelegateBuilder = new OperationPipelineBuilder(builder.ApplicationServices);
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
            .RunOperationExecutor(operation => operation.UseDefaults());
    }

    /// <summary>
    ///     Adds a named schema to the <see cref="QueryContext" /> from <see cref="SchemaCollection" />
    ///     resolved from registered services.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    public static GraphQLRequestPipelineBuilder UseSchema(this GraphQLRequestPipelineBuilder builder, string schemaName)
    {
        SchemaCollection schemaCollection = builder.ApplicationServices.GetRequiredService<SchemaCollection>();
        builder.Use(next => context =>
        {
            context.Schema = schemaCollection.Get(schemaName);
            return next(context);
        });

        return builder;
    }
}