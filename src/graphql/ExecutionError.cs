using System.Collections.Generic;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

public class ExecutionError
{
    public Dictionary<string, object> Extensions { get; set; }

    public List<Location>? Locations { get; set; }

    public string Message { get; set; }

    public List<object> Path { get; set; }

    public void Extend(string key, object value)
    {
        if (Extensions == null)
            Extensions = new Dictionary<string, object>();

        Extensions[key] = value;
    }
}