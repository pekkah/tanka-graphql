namespace Tanka.GraphQL.Internal;

public interface IPropertyAdapter<in T>
{
    string Name { get; }

    object? GetValue(T obj);

    void SetValue(T obj, object? value);
}

public class PropertyAdapter<T>(string name, Func<T, object?> getter, Action<T, object?> setter)
    : IPropertyAdapter<T>
{
    public string Name { get; } = name;

    private Func<T, object?> Getter { get; } = getter;

    private Action<T, object?> Setter { get; } = setter;

    public object? GetValue(T obj)
    {
        return Getter(obj);
    }

    public void SetValue(T obj, object? value)
    {
        Setter(obj, value);
    }
}