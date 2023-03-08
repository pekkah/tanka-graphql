using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Execution;
using Tanka.GraphQL.Extensions.Trace;
using Tanka.GraphQL.Features;

namespace Tanka.GraphQL;

public partial class Executor
{
    private static readonly IServiceProvider EmptyProvider = new ServiceCollection().BuildServiceProvider();

    private readonly OperationDelegate _operationDelegate;

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
            }).AddDefaultFeatures();

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

    public Executor(OperationDelegate operationDelegate)
    {
        _operationDelegate = operationDelegate;
    }
}