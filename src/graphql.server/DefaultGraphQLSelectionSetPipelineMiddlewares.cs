using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLSelectionSetPipelineMiddlewares
{
    public static GraphQLSelectionSetPipelineBuilder UseSelectionSetExecutor(
        this GraphQLSelectionSetPipelineBuilder builder)
    {
        var executor = builder.ApplicationServices.GetRequiredService<ISelectionSetExecutor>();

        builder.Use(_ => async context =>
        {
            context.Result =
                await executor.ExecuteSelectionSet(context.QueryContext,
                    context.SelectionSet,
                    context.ObjectDefinition,
                    context.ObjectValue,
                    context.Path);
        });

        return builder;
    }
}