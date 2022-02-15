using System.Collections.Generic;

namespace Tanka.GraphQL.Validation;

public class ExtensionData
{
    private readonly Dictionary<string, object> _data = new();

    public IReadOnlyDictionary<string, object> Data => _data;

    public void Set(string key, object data)
    {
        _data[key] = data;
    }
}