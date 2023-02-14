using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;
using Tanka.GraphQL.Validation;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL;

public partial class Executor
{
    private readonly FeatureCollection _defaults = new(1);
    private readonly ILogger<Executor> _logger;

    public Executor(
        ISchema schema,
        IOperationExecutorFeature operationExecutor,
        ISelectionSetExecutorFeature selectionSetExecutor,
        IFieldExecutorFeature fieldExecutor,
        ILogger<Executor> logger)
    {
        _defaults.Set<ISchemaFeature>(new SchemaFeature
        {
            Schema = schema
        });

        _defaults.Set<IOperationExecutorFeature>(operationExecutor);
        _defaults.Set<ISelectionSetExecutorFeature>(selectionSetExecutor);
        _defaults.Set<IFieldExecutorFeature>(fieldExecutor);

        _defaults.Set<IValidatorFeature>(new ValidatorFeature()
        {
            Validator = new Validator3(ExecutionRules.All)
        });

        _defaults.Set<IValueCompletionFeature>(new ValueCompletionFeature());
        _defaults.Set<IErrorCollectorFeature>(new ConcurrentBagErrorCollectorFeature());
        _defaults.Set<IArgumentBinderFeature>(new ArgumentBinderFeature());

        _logger = logger;
    }

    public Executor(ISchema schema, ILogger<Executor> logger) : this(
        schema,
        new OperationExecutorFeature(),
        new SelectionSetExecutorFeature(),
        new FieldExecutorFeature(),
        logger)
    {
    }

    public Executor(ISchema schema) : this(schema, new NullLogger<Executor>())
    {
    }

    public Executor(ILogger<Executor> logger)
    {
        _logger = logger;
    }
}