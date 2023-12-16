using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL;

public static class PrettyJsonLog
{
    private static readonly JsonSerializerOptions Pretty = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static string PrettyJson<T>(T value)
    {
        return JsonSerializer.Serialize<T>(value, Pretty);
    }
}