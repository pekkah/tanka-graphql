using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL;

public class OperationPipelineBuilder
{
    private readonly List<Func<OperationDelegate, OperationDelegate>> _components = new();

    protected OperationPipelineBuilder(OperationPipelineBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected OperationPipelineBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public OperationPipelineBuilder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IDictionary<string, object?> Properties { get; }

    public OperationDelegate Build()
    {
        OperationDelegate pipeline = (_, _) => throw new QueryException(
            "Operation execution pipeline error. No ending middleware.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }


    public OperationPipelineBuilder New()
    {
        return new OperationPipelineBuilder(this);
    }

    public OperationPipelineBuilder Use(Func<OperationDelegate, OperationDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    protected T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out object? value) ? (T?)value : default(T?);
    }

    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }
}