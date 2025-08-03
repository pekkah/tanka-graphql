using Microsoft.Extensions.DependencyInjection;

using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL.SelectionSets;

public class SelectionSetPipelineBuilder
{
    private const string ApplicationServicesKey = "ApplicationServices";

    private readonly List<Func<SelectionSetDelegate, SelectionSetDelegate>> _components = new();

    protected SelectionSetPipelineBuilder(SelectionSetPipelineBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected SelectionSetPipelineBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public SelectionSetPipelineBuilder(IServiceProvider applicationServices)
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        SetProperty(ApplicationServicesKey, applicationServices);
    }

    public IDictionary<string, object?> Properties { get; }

    public IServiceProvider ApplicationServices => GetRequiredProperty<IServiceProvider>(ApplicationServicesKey);

    public SelectionSetDelegate Build()
    {
        SelectionSetDelegate pipeline = _ => throw new QueryException(
            "SelectionSet execution pipeline error. No middleware set any results.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    public SelectionSetPipelineBuilder New()
    {
        return new SelectionSetPipelineBuilder(this);
    }

    public SelectionSetPipelineBuilder RunExecute()
    {
        return Use(_ => async context =>
        {
            context.Result = await DefaultSelectionSetExecutorFeature.ExecuteSelectionSet(
                context.QueryContext,
                context.GroupedFieldSet,
                context.ObjectDefinition,
                context.ObjectValue,
                context.Path);
        });
    }

    public SelectionSetPipelineBuilder Use(Func<SelectionSetDelegate, SelectionSetDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public SelectionSetPipelineBuilder UseFieldCollector()
    {
        return Use(next => context =>
        {
            var fieldCollector = ApplicationServices.GetRequiredService<IFieldCollector>();
            var collectionResult = fieldCollector.CollectFields(
                context.QueryContext.Schema,
                context.QueryContext.Request.Query,
                context.ObjectDefinition,
                context.SelectionSet,
                context.QueryContext.CoercedVariableValues);
            context.GroupedFieldSet = collectionResult.Fields;

            return next(context);
        });
    }

    public T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out object? value) ? (T?)value : default(T?);
    }

    public T GetRequiredProperty<T>(string key)
    {
        T? value = GetProperty<T>(key);

        ArgumentNullException.ThrowIfNull(value);

        return value;
    }

    public void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }
}