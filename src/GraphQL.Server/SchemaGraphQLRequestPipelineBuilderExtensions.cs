using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tanka.GraphQL.Server;

public static partial class SchemaGraphQLRequestPipelineBuilderExtensions
{
    [LoggerMessage(1, LogLevel.Information, "Using schema: '{schemaName}'")]
    public static partial void LogUsesSchema(ILogger logger, string schemaName);

    /// <summary>
    ///     Adds a named schema to the <see cref="QueryContext" /> from <see cref="SchemaCollection" />
    ///     resolved from registered services.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    public static GraphQLRequestPipelineBuilder UseSchema(this GraphQLRequestPipelineBuilder builder, string schemaName)
    {
        ILogger logger = builder.ApplicationServices.GetRequiredService<ILogger<SchemaCollection>>();
        SchemaCollection schemaCollection = builder.ApplicationServices.GetRequiredService<SchemaCollection>();
        builder.Use(next => context =>
        {
            LogUsesSchema(logger, schemaName);
            context.Schema = schemaCollection.Get(schemaName);
            return next(context);
        });

        return builder;
    }
}