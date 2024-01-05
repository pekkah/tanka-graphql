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
    public List<SerializedLocation>? Locations { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

    [JsonPropertyName("path")] public object[] Path { get; set; } = Array.Empty<object>();

    public void Extend(string key, object value)
    {
        Extensions ??= new();

        Extensions[key] = value;
    }
}

public class SerializedLocation
{
    public int Line { get; set; }
    
    public int Column { get; set; }

    public static implicit operator SerializedLocation(Location location)  
    {
        return new()
        {
            Line = location.Line,
            Column = location.Column
        };
    }
}

public static class ExecutionErrorExtensions
{
    public static List<SerializedLocation> ToSerializedLocations(this IEnumerable<Location> locations)
    {
        return [.. locations];
    }
}