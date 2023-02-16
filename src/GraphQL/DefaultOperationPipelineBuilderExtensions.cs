using Tanka.GraphQL.Features;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public static class DefaultOperationPipelineBuilderExtensions
{
    public static OperationPipelineBuilder UseDefaultErrorCollector(
        this OperationPipelineBuilder builder)
    {
        var feature = new ConcurrentBagErrorCollectorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IErrorCollectorFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder UseDefaultFieldPipeline(
        this OperationPipelineBuilder builder)
    {
        var feature = new FieldExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IFieldExecutorFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder UseDefaultOperationExecutor(
        this OperationPipelineBuilder builder)
    {
        var feature = new DefaultOperationExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IOperationExecutorFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder UseDefaultOperationResolver(this OperationPipelineBuilder builder)
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

    /// <summary>
    ///     Use default execution features and pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static OperationPipelineBuilder UseDefaults(this OperationPipelineBuilder builder)
    {
        builder.UseDefaultErrorCollector();
        builder.UseDefaultOperationExecutor();
        builder.UseDefaultOperationResolver();
        builder.UseDefaultVariableCoercer();
        builder.UseDefaultValidator();
        builder.UseDefaultSelectionSetPipeline();
        builder.UseDefaultFieldPipeline();
        builder.UseDefaultValueCompletion();
        builder.RunOperation();

        return builder;
    }

    public static OperationPipelineBuilder UseDefaultSelectionSetPipeline(
        this OperationPipelineBuilder builder)
    {
        return builder.UseSelectionSetPipeline(pipe => pipe.UseFieldCollector().RunExecute());
    }

    public static OperationPipelineBuilder UseDefaultValidator(this OperationPipelineBuilder builder)
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

    public static OperationPipelineBuilder UseDefaultValueCompletion(
        this OperationPipelineBuilder builder)
    {
        var feature = new ValueCompletionFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IValueCompletionFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder UseDefaultVariableCoercer(this OperationPipelineBuilder builder)
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
}