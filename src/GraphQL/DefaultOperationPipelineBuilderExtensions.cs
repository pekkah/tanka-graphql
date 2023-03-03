using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public static class DefaultOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder AddFeature<TFeature>(this OperationDelegateBuilder builder, TFeature feature)
    {
        return builder.Use(next => context =>
        {
            context.Features.Set<TFeature>(feature);
            return next(context);
        });
    }

    public static OperationDelegateBuilder AddDefaultErrorCollectorFeature(
        this OperationDelegateBuilder builder)
    {
        var feature = new ConcurrentBagErrorCollectorFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IErrorCollectorFeature>(feature);
            return next(context);
        });
    }



    public static OperationDelegateBuilder AddDefaultArgumentBinderFeature(
        this OperationDelegateBuilder builder)
    {
        var feature = new ArgumentBinderFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IArgumentBinderFeature>(feature);
            return next(context);
        });
    }

    public static OperationDelegateBuilder UseDefaultOperationResolver(this OperationDelegateBuilder builder)
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
    public static OperationDelegateBuilder UseDefaults(this OperationDelegateBuilder builder)
    {
        if (builder.GetProperty<bool>("TraceEnabled"))
        {
            builder.UseTrace();
        }

        // extend query context with required features
        builder.AddDefaultErrorCollectorFeature();
        
        builder.AddDefaultSelectionSetExecutorFeature();
        builder.AddDefaultFieldExecutorFeature();
        builder.AddDefaultValueCompletionFeature();

        // actual flow
        builder.UseDefaultValidator();
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

    

    public static OperationDelegateBuilder UseDefaultValidator(this OperationDelegateBuilder builder)
    {
        var validator = new Validator3(ExecutionRules.All);

        builder.Use(next => async context =>
        {
            var result = await validator.Validate(context.Schema, context.Request.Document, context.Request.Variables);

            if (!result.IsValid)
                throw new ValidationException(result);

            await next(context);
        });

        return builder;
    }

    public static OperationDelegateBuilder AddDefaultValueCompletionFeature(
        this OperationDelegateBuilder builder)
    {
        var feature = new ValueCompletionFeature();
        return builder.Use(next => context =>
        {
            context.Features.Set<IValueCompletionFeature>(feature);
            return next(context);
        });
    }

    public static OperationDelegateBuilder UseDefaultVariableCoercer(this OperationDelegateBuilder builder)
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

    public static OperationDelegateBuilder WhenOperationTypeUse(
        this OperationDelegateBuilder builder,
        Action<OperationDelegateBuilder> queryAction,
        Action<OperationDelegateBuilder> mutationAction,
        Action<OperationDelegateBuilder> subscriptionAction)
    {
        var queryBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        queryAction(queryBuilder);
        var queryDelegate = queryBuilder.Build();

        var mutationBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
        mutationAction(mutationBuilder);
        var mutationDelegate = mutationBuilder.Build();

        var subscriptionBuilder = new OperationDelegateBuilder(builder.ApplicationServices);
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

    public static OperationDelegateBuilder RunQueryOrMutation(this OperationDelegateBuilder builder)
    {
        return builder.Use(_ => async context =>
        {
            await Executor.ExecuteQueryOrMutation(context);
        });
    }

    public static OperationDelegateBuilder RunSubscription(this OperationDelegateBuilder builder)
    {
        return builder.Use(_ => async context =>
        {
            await Executor.ExecuteSubscription(context);
        });
    }
}