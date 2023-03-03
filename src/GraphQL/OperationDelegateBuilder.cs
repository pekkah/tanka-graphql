using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL;

public class OperationDelegateBuilder
{
    private const string ApplicationServicesKey = "ApplicationServices";
    private readonly List<Func<OperationDelegate, OperationDelegate>> _components = new();

    protected OperationDelegateBuilder(OperationDelegateBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected OperationDelegateBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public OperationDelegateBuilder(IServiceProvider applicationServices)
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        SetProperty(ApplicationServicesKey, applicationServices);
    }

    public IDictionary<string, object?> Properties { get; }

    public IServiceProvider ApplicationServices => GetRequiredProperty<IServiceProvider>(ApplicationServicesKey);

    public OperationDelegate Build()
    {
        OperationDelegate pipeline = _ => throw new QueryException(
            "Operation execution pipeline error. No ending middleware.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    public OperationDelegateBuilder Clone()
    {
        var clone = new OperationDelegateBuilder(this);
        clone._components.AddRange(_components);
        return clone;
    }

    public T? GetProperty<T>(string key, T? defaultValue = default)
    {
        return Properties.TryGetValue(key, out object? value) ? (T?)value : defaultValue;
    }

    public T GetRequiredProperty<T>(string key)
    {
        T? value = GetProperty<T>(key);

        ArgumentNullException.ThrowIfNull(value);

        return value;
    }


    public OperationDelegateBuilder New()
    {
        return new OperationDelegateBuilder(this);
    }

    public void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }

    public OperationDelegateBuilder Use(Func<OperationDelegate, OperationDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }
}