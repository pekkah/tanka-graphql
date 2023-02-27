using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public static class DefaultOperationPipelineBuilderExtensions
{
    public static OperationPipelineBuilder AddFeature<TFeature>(this OperationPipelineBuilder builder, TFeature feature)
    {
        return builder.Use(next => context =>
        {
            context.Features.Set<TFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder AddDefaultErrorCollectorFeature(
        this OperationPipelineBuilder builder)
    {
        var feature = new ConcurrentBagErrorCollectorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IErrorCollectorFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder AddDefaultFieldPipelineFeature(
        this OperationPipelineBuilder builder)
    {
        var feature = new FieldExecutorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IFieldExecutorFeature>(feature);
            return next(context);
        });
    }

    public static OperationPipelineBuilder AddDefaultArgumentBinderFeature(
        this OperationPipelineBuilder builder)
    {
        var feature = new ArgumentBinderFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IArgumentBinderFeature>(feature);
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
        builder.AddDefaultErrorCollectorFeature();
        builder.AddDefaultFieldPipelineFeature();
        builder.AddDefaultValidator();
        builder.AddDefaultSelectionSetPipeline();
        builder.AddDefaultValueCompletion();

        builder.UseDefaultOperationResolver();
        builder.UseDefaultVariableCoercer();
        
        builder.WhenOperationTypeUse(query =>
            {
                query.RunQueryOrMutation();
            },
            mutation =>
            {
                mutation.RunQueryOrMutation();
            },
            subscriptions =>
            {
                subscriptions.RunSubscription();
            });

        return builder;
    }

    public static OperationPipelineBuilder AddDefaultSelectionSetPipeline(
        this OperationPipelineBuilder builder)
    {
        return builder.AddSelectionSetPipeline(pipe => pipe.UseFieldCollector().RunExecute());
    }

    public static OperationPipelineBuilder AddDefaultValidator(this OperationPipelineBuilder builder)
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

    public static OperationPipelineBuilder AddDefaultValueCompletion(
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

    public static OperationPipelineBuilder WhenOperationTypeUse(
        this OperationPipelineBuilder builder,
        Action<OperationPipelineBuilder> queryAction,
        Action<OperationPipelineBuilder> mutationAction,
        Action<OperationPipelineBuilder> subscriptionAction)
    {
        var queryBuilder = new OperationPipelineBuilder(builder.ApplicationServices);
        queryAction(queryBuilder);
        var queryDelegate = queryBuilder.Build();

        var mutationBuilder = new OperationPipelineBuilder(builder.ApplicationServices);
        mutationAction(mutationBuilder);
        var mutationDelegate = mutationBuilder.Build();

        var subscriptionBuilder = new OperationPipelineBuilder(builder.ApplicationServices);
        subscriptionAction(subscriptionBuilder);
        var subscriptionDelegate = subscriptionBuilder.Build();

        return builder.Use(_ => async context =>
        {
            Task execute = context.OperationDefinition.Operation switch
            {
                OperationType.Query => queryDelegate(context),
                OperationType.Mutation => mutationDelegate(context),
                OperationType.Subscription => subscriptionDelegate(context),
                _ => throw new InvalidOperationException(
                    $"Operation type {context.OperationDefinition.Operation} not supported.")
            };

            await execute;
        });
    }

    public static OperationPipelineBuilder RunQueryOrMutation(this OperationPipelineBuilder builder)
    {
        return builder.Use(_ => async context =>
        {
            await Executor.ExecuteQueryOrMutation(context);
        });
    }

    public static OperationPipelineBuilder RunSubscription(this OperationPipelineBuilder builder)
    {
        return builder.Use(_ => async context =>
        {
            await Executor.ExecuteSubscription(context);
        });
    }
}