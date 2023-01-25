using System;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLRequestPipelineMiddlewares
{
    public static GraphQLRequestPipelineBuilder UseSchema(this GraphQLRequestPipelineBuilder builder, string schemaName)
    {
        var schemaCollection = builder.ApplicationServices.GetRequiredService<SchemaCollection>();
        builder.Use(next => context =>
        {
            context.Schema = schemaCollection.Get(schemaName);
            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder UseDefaultOperationResolver(this GraphQLRequestPipelineBuilder builder)
    {
        builder.Use(next => context =>
        {
 
            context.OperationDefinition = Operations.GetOperation(
                context.Request.Document,
                context.Request.OperationName);

            context.CoercedVariableValues = Variables.CoerceVariableValues(
                context.Schema,
                context.OperationDefinition,
                context.Request.VariableValues);

            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder UseDefaultVariableCoercer(this GraphQLRequestPipelineBuilder builder)
    {
        builder.Use(next => context =>
        {
            context.CoercedVariableValues = Variables.CoerceVariableValues(
                context.Schema,
                context.OperationDefinition,
                context.Request.VariableValues);

            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder UseSelectionSetPipeline(
        this GraphQLRequestPipelineBuilder builder, 
        Action<GraphQLSelectionSetPipelineBuilder> configurePipeline)
    {
        var selectionSetPipelineBuilder = new GraphQLSelectionSetPipelineBuilder(builder.ApplicationServices);
        configurePipeline(selectionSetPipelineBuilder);
        var feature = new SelectionSetPipelineExecutorFeature(selectionSetPipelineBuilder.Build());
        builder.Use(next => context =>
        {
            context.Features.Set<ISelectionSetExecutorFeature>(feature);

            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder RunExecutor(this GraphQLRequestPipelineBuilder builder)
    {
        var executor = builder.ApplicationServices.GetRequiredService<Executor>();
        builder.Use(_ => context => executor.Subscribe(context));

        return builder;
    }
}