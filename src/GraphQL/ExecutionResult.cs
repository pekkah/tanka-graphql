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
    private IReadOnlyList<IncrementalPayload>? _incremental;

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(NestedDictionaryConverter))]
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

    /// <summary>
    /// Indicates whether there are more results to come for incremental delivery
    /// </summary>
    [JsonPropertyName("hasNext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasNext { get; init; }

    /// <summary>
    /// List of incremental payloads for @defer and @stream
    /// </summary>
    [JsonPropertyName("incremental")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<IncrementalPayload>? Incremental
    {
        get => _incremental;
        set
        {
            if (value != null && !value.Any())
            {
                _incremental = null;
                return;
            }

            _incremental = value;
        }
    }

    public override string ToString()
    {
        return PrettyJsonLog.PrettyJson(this);
    }
}

/// <summary>
/// Payload for incremental delivery (@defer and @stream)
/// </summary>
public record IncrementalPayload
{
    /// <summary>
    /// Optional label for the deferred fragment
    /// </summary>
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; init; }

    /// <summary>
    /// Path to the field being deferred/streamed
    /// </summary>
    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(PathConverter))]
    public NodePath? Path { get; init; }

    /// <summary>
    /// Data for this incremental payload (@defer)
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object?>? Data { get; init; }

    /// <summary>
    /// Items for this incremental payload (@stream)
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<object?>? Items { get; init; }

    /// <summary>
    /// Errors for this incremental payload
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ExecutionError>? Errors { get; init; }
}