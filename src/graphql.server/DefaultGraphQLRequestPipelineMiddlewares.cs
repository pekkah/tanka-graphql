using System;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;

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
                context.Request.Variables);

            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder UseSelectionSetPipeline(
        this GraphQLRequestPipelineBuilder builder,
        Action<GraphQLSelectionSetPipelineBuilder> configurePipeline,
        bool runSelectionSetExecutorAtEnd = true)
    {
        var selectionSetPipelineBuilder = new GraphQLSelectionSetPipelineBuilder(builder.ApplicationServices);
        configurePipeline(selectionSetPipelineBuilder);

        // append the end last middleware automatically
        if (runSelectionSetExecutorAtEnd)
            selectionSetPipelineBuilder.RunSelectionSetExecutor();

        var feature = new SelectionSetPipelineExecutorFeature(selectionSetPipelineBuilder.Build());

        builder.Use(next => context =>
        {
            context.Features.Set<ISelectionSetExecutorFeature>(feature);
            return next(context);
        });

        return builder;
    }

    public static GraphQLRequestPipelineBuilder UseDefaultSelectionSetPipeline(
        this GraphQLRequestPipelineBuilder builder)
    {
        return builder.UseSelectionSetPipeline(
            pipe => { }, 
            true);
    }

    public static GraphQLRequestPipelineBuilder UseDefaultValidator(this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new ValidatorFeature()
        {
            Validator = new Validator3(ExecutionRules.All)
        };

        builder.Use(next => context =>
        {
            context.Features.Set<IValidatorFeature>(feature);
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