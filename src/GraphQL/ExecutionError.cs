using System.Text.Json.Serialization;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL;

public class ExecutionError
{
    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Extensions { get; set; }

    [JsonPropertyName("locations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Location>? Locations { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("path")] public object[] Path { get; set; } = Array.Empty<object>();

    public void Extend(string key, object value)
    {
        Extensions ??= new();

        Extensions[key] = value;
    }
}