using Microsoft.AspNetCore.Http.Features;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    private readonly FeatureCollection _defaults = new(8);

    /// <summary>
    ///     Provide all the features required for query execution. Alternative is to
    ///     use <see cref="OperationPipelineBuilder" /> and set the <see cref="OperationPipelineExecutorFeature" />
    ///     for the <see cref="QueryContext" />.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="operationExecutor"></param>
    /// <param name="selectionSetExecutor"></param>
    /// <param name="fieldExecutor"></param>
    /// <param name="validator"></param>
    /// <param name="valueCompletion"></param>
    public Executor(
        ISchema schema,
        IOperationExecutorFeature operationExecutor,
        ISelectionSetExecutorFeature selectionSetExecutor,
        IFieldExecutorFeature fieldExecutor,
        IValidatorFeature validator,
        IValueCompletionFeature valueCompletion)
    {
        _defaults.Set<ISchemaFeature>(new SchemaFeature
        {
            Schema = schema
        });

        _defaults.Set(operationExecutor);
        _defaults.Set(validator);
        _defaults.Set(selectionSetExecutor);
        _defaults.Set(fieldExecutor);
        _defaults.Set(valueCompletion);

        _defaults.Set<IErrorCollectorFeature>(new ConcurrentBagErrorCollectorFeature());
        _defaults.Set<IArgumentBinderFeature>(new ArgumentBinderFeature());
    }

    /// <summary>
    ///     Create executor with sane defaults and use given <see cref="ISchema" />.
    /// </summary>
    /// <param name="schema"></param>
    public Executor(ISchema schema) : this(
        schema,
        new DefaultOperationExecutorFeature(),
        new SelectionSetExecutorFeature(),
        new FieldExecutorFeature(),
        new ValidatorFeature
        {
            Validator = new Validator3(ExecutionRules.All)
        },
        new ValueCompletionFeature())
    {
    }

    /// <summary>
    ///     Create executor without any defaults.
    ///     When executing queries the <see cref="QueryContext" /> must provide
    ///     the features required for the execution of the query or the query will fail.
    /// </summary>
    public Executor()
    {
    }
}