using Microsoft.Extensions.DependencyInjection;

namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLRequestPipelineMiddlewares
{
    public static GraphQLRequestPipelineBuilder RunOperationPipeline(
        this GraphQLRequestPipelineBuilder builder,
        Action<OperationPipelineBuilder> configureOperation)
    {
        var operationBuilder = new OperationPipelineBuilder(builder.ApplicationServices);
        configureOperation(operationBuilder);
        OperationDelegate operationDelegate = operationBuilder.Build();

        return builder.Use(_ => async context => { await operationDelegate(context); });
    }

    public static GraphQLRequestPipelineBuilder UseDefaults(this GraphQLRequestPipelineBuilder builder,
        string schemaName)
    {
        return builder
            .UseSchema(schemaName)
            .RunOperationPipeline(operation => operation.UseDefaults());
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