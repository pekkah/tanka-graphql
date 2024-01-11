using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public static class DefaultOperationDelegateBuilderExtensions
{
    public static OperationDelegateBuilder UseDefaultRequestServices(this OperationDelegateBuilder builder)
    {
        return builder.Use(next => async context =>
        {
            await using AsyncServiceScope scope = builder.ApplicationServices.CreateAsyncScope();
            context.RequestServices = scope.ServiceProvider;
            await next(context);
        });
    }

    public static OperationDelegateBuilder AddDefaultFeatures(
        this OperationDelegateBuilder builder)
    {
        var argumentBinderFeature = new ArgumentBinderFeature();
        var defaultSelectionSetExecutorFeature = new DefaultSelectionSetExecutorFeature();
        var fieldExecutorFeature = new FieldExecutorFeature();
        var valueCompletionFeature = new ValueCompletionFeature();

        return builder.Use(next => context =>
        {
            // errors use state
            context.Features.Set<IErrorCollectorFeature>(new ConcurrentBagErrorCollectorFeature());
            context.Features.Set<IArgumentBinderFeature>(argumentBinderFeature);
            context.Features.Set<ISelectionSetExecutorFeature>(defaultSelectionSetExecutorFeature);
            context.Features.Set<IFieldExecutorFeature>(fieldExecutorFeature);
            context.Features.Set<IValueCompletionFeature>(valueCompletionFeature);
            return next(context);
        });
    }

    public static OperationDelegateBuilder RunQueryOrMutation(this OperationDelegateBuilder builder)
    {
        return builder.Use(_ => async context => { await Executor.ExecuteQueryOrMutation(context); });
    }

    public static OperationDelegateBuilder RunSubscription(this OperationDelegateBuilder builder)
    {
        return builder.Use(_ => async context => { await Executor.ExecuteSubscription(context); });
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
        if (builder.GetProperty<bool>("TraceEnabled")) builder.UseTrace();
        builder.UseDefaultRequestServices();

        // extend query context with required features
        builder.AddDefaultFeatures();

        // actual flow
        builder.UseDefaultValidator();
        builder.UseDefaultOperationResolver();
        builder.UseDefaultVariableCoercer();
        builder.WhenOperationTypeIsUse(
            query => { query.RunQueryOrMutation(); },
            mutation => { mutation.RunQueryOrMutation(); },
            subscriptions => { subscriptions.RunSubscription(); }
            );

        return builder;
    }


    public static OperationDelegateBuilder UseDefaultValidator(this OperationDelegateBuilder builder)
    {
        var validator = builder.ApplicationServices.GetRequiredService<IAsyncValidator>();

        builder.Use(next => async context =>
        {
            ValidationResult result =
                await validator.Validate(context.Schema, context.Request.Document, context.Request.Variables);

            if (!result.IsValid)
                throw new ValidationException(result);

            await next(context);
        });

        return builder;
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
}