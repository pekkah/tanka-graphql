using System;
using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Server;

public static class DefaultGraphQLRequestPipelineMiddlewares
{
    public static GraphQLRequestPipelineBuilder RunOperation(this GraphQLRequestPipelineBuilder builder)
    {
        builder.Use(_ => context => context.ExecuteOperation());

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

    public static GraphQLRequestPipelineBuilder UseDefaultOperationExecutor(
        this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new OperationExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IOperationExecutorFeature>(feature);
            return next(context);
        });
    }

    public static GraphQLRequestPipelineBuilder UseDefaultValueCompletion(
        this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new ValueCompletionFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IValueCompletionFeature>(feature);
            return next(context);
        });
    }

    public static GraphQLRequestPipelineBuilder UseDefaultFieldPipeline(
        this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new FieldExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IFieldExecutorFeature>(feature);
            return next(context);
        });
    }

    public static GraphQLRequestPipelineBuilder UseDefaultErrorCollector(
        this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new ConcurrentBagErrorCollectorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IErrorCollectorFeature>(feature);
            return next(context);
        });
    }

    public static GraphQLRequestPipelineBuilder UseDefaultSelectionSetPipeline(
        this GraphQLRequestPipelineBuilder builder)
    {
        return builder.UseSelectionSetPipeline(pipe => pipe.UseFieldCollector().RunExecute());
    }

    public static GraphQLRequestPipelineBuilder UseDefaultValidator(this GraphQLRequestPipelineBuilder builder)
    {
        var feature = new ValidatorFeature
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

    public static GraphQLRequestPipelineBuilder UseSelectionSetPipeline(
        this GraphQLRequestPipelineBuilder builder,
        Action<SelectionSetPipelineBuilder> configurePipeline)
    {
        var selectionSetPipelineBuilder = new SelectionSetPipelineBuilder(builder.ApplicationServices);
        configurePipeline(selectionSetPipelineBuilder);

        var feature = new SelectionSetPipelineExecutorFeature(selectionSetPipelineBuilder.Build());

        builder.Use(next => context =>
        {
            context.Features.Set<ISelectionSetExecutorFeature>(feature);
            return next(context);
        });

        return builder;
    }
}