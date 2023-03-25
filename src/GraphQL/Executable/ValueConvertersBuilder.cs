using Tanka.GraphQL.ValueSerialization;

namespace Tanka.GraphQL.Executable;

public class ValueConvertersBuilder
{
    public Dictionary<string, IValueConverter> ValueConverters { get; } = new();

    public ValueConvertersBuilder AddDefaults()
    {
        ValueConverters.Add("Int", new IntConverter());
        ValueConverters.Add("Float", new DoubleConverter());
        ValueConverters.Add("String", new StringConverter());
        ValueConverters.Add("Boolean", new BooleanConverter());
        ValueConverters.Add("ID", new IdConverter());

        return this;
    }

    public ValueConvertersBuilder Add(string type, IValueConverter converter)
    {
        ValueConverters.Add(type, converter);
        return this;
    }

    public IReadOnlyDictionary<string, IValueConverter> Build()
    {
        return ValueConverters;
    }
}