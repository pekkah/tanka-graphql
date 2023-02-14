using System;
using System.Collections.Generic;
using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL.Server;

public class GraphQLRequestPipelineBuilder
{
    private const string ApplicationServicesKey = "ApplicationServices";
    private List<Func<GraphQLRequestDelegate, GraphQLRequestDelegate>> _components = new();

    protected GraphQLRequestPipelineBuilder(GraphQLRequestPipelineBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected GraphQLRequestPipelineBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public GraphQLRequestPipelineBuilder(IServiceProvider applicationServices)
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        SetProperty(ApplicationServicesKey, applicationServices);
    }

    public IDictionary<string, object?> Properties { get; }

    public IServiceProvider ApplicationServices => GetRequiredProperty<IServiceProvider>(ApplicationServicesKey);


    public GraphQLRequestDelegate Build()
    {
        GraphQLRequestDelegate pipeline = _ => throw new QueryException(
            "Request execution pipeline error. No middleware returned any results.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    public GraphQLRequestPipelineBuilder Clone()
    {
        var clone = new GraphQLRequestPipelineBuilder(this)
        {
            _components = _components
        };

        return clone;
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

    public GraphQLRequestPipelineBuilder Use(Func<GraphQLRequestDelegate, GraphQLRequestDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }
}