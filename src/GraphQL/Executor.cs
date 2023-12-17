using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Features;

namespace Tanka.GraphQL;

/// <summary>
///     GraphQL operation executor.
/// </summary>
public partial class Executor
{
    private static readonly IServiceProvider EmptyProvider = new ServiceCollection().BuildServiceProvider();

    private readonly OperationDelegate _operationDelegate;

    /// <summary>
    ///     Create executor with defaults <see cref="OperationDelegate" /> using the given <see cref="ISchema" />.
    /// </summary>
    /// <param name="schema"></param>
    public Executor(ISchema schema) : this(new ExecutorOptions { Schema = schema })
    {
    }

    /// <summary>
    ///     Create executor with <see cref="OperationDelegate" /> created using the given <paramref name="options" />
    /// </summary>
    /// <param name="options"></param>
    public Executor(ExecutorOptions options)
    {
        OperationDelegateBuilder builder = new(options.ServiceProvider ?? EmptyProvider);

        if (options.TraceEnabled)
            builder.UseTrace();

        builder.UseDefaultRequestServices();

        builder.AddFeature<ISchemaFeature>(new SchemaFeature { Schema = options.Schema });

        builder.AddDefaultFeatures();

        if (options.ValidationEnabled)
            builder
                .UseDefaultValidator();

        builder
            .UseDefaultOperationResolver()
            .UseDefaultVariableCoercer()
            .WhenOperationTypeIsUse(
                q => q.RunQueryOrMutation(),
                m => m.RunQueryOrMutation(),
                s => s.RunSubscription())
            .Build();

        _operationDelegate = builder.Build();
    }

    /// <summary>
    ///     Create executor with given <paramref name="operationDelegate" /> .
    /// </summary>
    /// <param name="operationDelegate"></param>
    public Executor(OperationDelegate operationDelegate)
    {
        _operationDelegate = operationDelegate;
    }
}