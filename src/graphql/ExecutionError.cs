using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

public class ExecutionError
{
    public Dictionary<string, object>? Extensions { get; set; }

    public List<Location>? Locations { get; set; }

    public required string Message { get; set; }

    public object[] Path { get; set; }

    public void Extend(string key, object value)
    {
        Extensions ??= new();

        Extensions[key] = value;
    }
}