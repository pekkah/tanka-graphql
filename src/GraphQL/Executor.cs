using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tanka.GraphQL.Features;
using Tanka.GraphQL.SelectionSets;

namespace Tanka.GraphQL;

public partial class Executor
{
    private readonly FeatureCollection _defaults = new(1);
    private readonly ILogger<Executor> _logger;

    public Executor(
        ISchema schema,
        ISelectionSetExecutor selectionSetExecutor,
        IFieldExecutor fieldExecutor,
        ILogger<Executor> logger)
    {
        _defaults.Set<ISchemaFeature>(new SchemaFeature
        {
            Schema = schema
        });
        _defaults.Set<IFieldExecutorFeature>(new FieldExecutorFeature
        {
            FieldExecutor = fieldExecutor
        });

        _defaults.Set<ISelectionSetExecutorFeature>(new SelectionSetExecutorFeature(async context => context.Result = await selectionSetExecutor.ExecuteSelectionSet(
            context.QueryContext,
            context.SelectionSet,
            context.ObjectDefinition,
            context.ObjectValue,
            context.Path)));

        _logger = logger;
    }

    public Executor(ISchema schema, ILogger<Executor> logger) : this(
        schema, 
        ISelectionSetExecutor.Default,
        IFieldExecutor.Default, logger)
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