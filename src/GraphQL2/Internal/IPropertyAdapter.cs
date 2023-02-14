namespace Tanka.GraphQL.Internal;

public interface IPropertyAdapter<in T>
{ 
    string Name { get; }

    object? GetValue(T obj);

    void SetValue(T obj, object? value);
}

public class PropertyAdapter<T>: IPropertyAdapter<T>
{
    public PropertyAdapter(string name, Func<T, object?> getter, Action<T, object?> setter)
    {
        Name = name;
        Getter = getter;
        Setter = setter;
    }

    public string Name { get; }

    private Func<T, object?> Getter { get; }

    private Action<T, object?> Setter { get;  }

    public object? GetValue(T obj)
    {
        return Getter(obj);
    }

    public void SetValue(T obj, object? value)
    {
        Setter(obj, value);
    }
}