using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    private static readonly IServiceProvider EmptyProvider = new ServiceCollection().BuildServiceProvider();

    private readonly OperationDelegate _operationDelegate;

    /// <summary>
    ///     Create executor with default execution pipeline using
    ///     the provided features.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="selectionSetExecutor"></param>
    /// <param name="fieldExecutor"></param>
    /// <param name="validator"></param>
    /// <param name="valueCompletion"></param>
    public Executor(
        ISchema schema,
        ISelectionSetExecutorFeature selectionSetExecutor,
        IFieldExecutorFeature fieldExecutor,
        IValueCompletionFeature valueCompletion)
    {
        _operationDelegate = new OperationDelegateBuilder(EmptyProvider)
            .AddFeature<ISchemaFeature>(new SchemaFeature
            {
                Schema = schema
            })
            .AddFeature(selectionSetExecutor)
            .AddFeature(fieldExecutor)
            .AddFeature(valueCompletion)
            .AddDefaultErrorCollectorFeature()
            .AddDefaultArgumentBinderFeature()
            .UseDefaultValidator()
            .UseDefaultOperationResolver()
            .UseDefaultVariableCoercer()
            .WhenOperationTypeUse(
                q => q.RunQueryOrMutation(),
                m => m.RunQueryOrMutation(),
                s => s.RunSubscription())
            .Build();
    }

    /// <summary>
    ///     Create executor with defaults and use given <see cref="ISchema" />.
    /// </summary>
    /// <param name="schema"></param>
    public Executor(ISchema schema) : this(new ExecutorOptions()
    {
        Schema = schema
    })
    {
    }

    public Executor(ExecutorOptions options)
    {
        OperationDelegateBuilder builder = new OperationDelegateBuilder(EmptyProvider);

        if (options.TraceEnabled)
            builder.UseTrace();
        
        builder.AddFeature<ISchemaFeature>(new SchemaFeature
            {
                Schema = options.Schema
            })
            .AddDefaultSelectionSetExecutorFeature()
            .AddDefaultFieldExecutorFeature()
            .AddDefaultErrorCollectorFeature()
            .AddDefaultArgumentBinderFeature()
            .AddDefaultValueCompletionFeature();

        if (options.ValidationEnabled)
            builder
                .UseDefaultValidator();

        builder
            .UseDefaultOperationResolver()
            .UseDefaultVariableCoercer()
            .WhenOperationTypeUse(
                q => q.RunQueryOrMutation(),
                m => m.RunQueryOrMutation(),
                s => s.RunSubscription())
            .Build();

        _operationDelegate = builder.Build();
    }

    public Executor(OperationDelegate operationDelegate)
    {
        _operationDelegate = operationDelegate;
    }
}

public record ExecutorOptions
{
    public required ISchema Schema { get; set; }

    public bool TraceEnabled { get; set; } = false;

    public bool ValidationEnabled { get; set; } = true;
}