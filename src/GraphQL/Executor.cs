using Microsoft.Extensions.DependencyInjection;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;
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
        IValidatorFeature validator,
        IValueCompletionFeature valueCompletion)
    {
        _operationDelegate = new OperationPipelineBuilder(EmptyProvider)
            .AddFeature<ISchemaFeature>(new SchemaFeature
            {
                Schema = schema
            })
            .AddFeature(validator)
            .AddFeature(selectionSetExecutor)
            .AddFeature(fieldExecutor)
            .AddFeature(valueCompletion)
            .AddDefaultErrorCollectorFeature()
            .AddDefaultArgumentBinderFeature()

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
    public Executor(ISchema schema) : this(
        schema,
        new SelectionSetExecutorFeature(),
        new FieldExecutorFeature(),
        new ValidatorFeature
        {
            Validator = new Validator3(ExecutionRules.All)
        },
        new ValueCompletionFeature())
    {
    }

    public Executor(OperationDelegate operationDelegate)
    {
        _operationDelegate = operationDelegate;
    }
}