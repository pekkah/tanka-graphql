using Tanka.GraphQL.Internal;

namespace Tanka.GraphQL.Fields;

public class FieldDelegateBuilder
{
    private const string ApplicationServicesKey = "ApplicationServices";

    private readonly List<Func<FieldDelegate, FieldDelegate>> _components = new();

    protected FieldDelegateBuilder(FieldDelegateBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected FieldDelegateBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    public FieldDelegateBuilder(IServiceProvider applicationServices)
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
        SetProperty(ApplicationServicesKey, applicationServices);
    }

    public IDictionary<string, object?> Properties { get; }

    public IServiceProvider ApplicationServices => GetRequiredProperty<IServiceProvider>(ApplicationServicesKey);

    public FieldDelegate Build()
    {
        FieldDelegate pipeline = _ => throw new QueryException(
            "Field execution pipeline error. No middleware set any results.")
        {
            Path = new NodePath()
        };

        for (int c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    public FieldDelegateBuilder New()
    {
        return new FieldDelegateBuilder(this);
    }


    public FieldDelegateBuilder Use(Func<FieldDelegate, FieldDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
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