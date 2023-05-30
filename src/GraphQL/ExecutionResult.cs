using System.Text.Json.Serialization;
using Tanka.GraphQL.Json;

namespace Tanka.GraphQL;

/// <summary>
///     Result of query, mutation or value of one value in the
///     subscription stream
/// </summary>
public record ExecutionResult
{
    private IReadOnlyDictionary<string, object?>? _data;
    private IReadOnlyList<ExecutionError>? _errors;
    private IReadOnlyDictionary<string, object>? _extensions;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object?>? Data
    {
        get => _data;
        set
        {
            if (value != null && !value.Any())
            {
                _data = null;
                return;
            }

            _data = value;
        }
    }

    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object>? Extensions
    {
        get => _extensions;
        set
        {
            if (value != null && !value.Any())
            {
                _extensions = null;
                return;
            }

            _extensions = value;
        }
    }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ExecutionError>? Errors
    {
        get => _errors;
        set
        {
            if (value != null)
                if (!value.Any())
                {
                    _errors = null;
                    return;
                }

            _errors = value;
        }
    }

    public override string ToString()
    {
        return PrettyJsonLog.PrettyJson(this);
    }
}