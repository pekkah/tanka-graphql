using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL;

public class NestedDictionaryConverter : JsonConverter<IReadOnlyDictionary<string, object?>>
{
    private readonly bool _useDecimals;

    public NestedDictionaryConverter(bool useDecimals = false)
    {
        _useDecimals = useDecimals;
    }

    public NestedDictionaryConverter():this(false)
    {
        
    }


    public override IReadOnlyDictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        EnsureToken(reader.TokenType, JsonTokenType.StartObject);

        var dictionary = new Dictionary<string, object?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) return dictionary;

            EnsureToken(reader.TokenType, JsonTokenType.PropertyName);


            var propertyName = reader.GetString();

            if (propertyName is null)
                throw new JsonException("Dictionary key must not be null");

            reader.Read();

            var value = ReadValue(ref reader, options);
            dictionary.Add(propertyName, value);
        }

        return dictionary;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyDictionary<string, object?> value,
        JsonSerializerOptions options)
    {
        foreach (var kv in value)
        {
            JsonSerializer.Serialize(writer, kv, options);
        }
    }

    private object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.None => null,
            JsonTokenType.StartObject => Read(ref reader, typeof(IReadOnlyDictionary<string, object?>), options),
            JsonTokenType.EndObject => throw new JsonException(),
            JsonTokenType.StartArray => ReadArray(ref reader, options),
            JsonTokenType.EndArray => throw new JsonException(),
            JsonTokenType.PropertyName => throw new JsonException(),
            JsonTokenType.Comment => SkipComment(ref reader, options),
            JsonTokenType.String => ReadString(ref reader),
            JsonTokenType.Number => ReadNumber(ref reader),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private object? ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        EnsureToken(reader.TokenType, JsonTokenType.StartArray);

        var list = new List<object?>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var value = ReadValue(ref reader, options);
            list.Add(value);
        }

        return list.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object? ReadNumber(ref Utf8JsonReader reader)
    {
        object? v = null;
        if (reader.TryGetInt32(out var i))
            v = i;

        if (reader.TryGetInt64(out var l))
            v = i;

        if (_useDecimals && reader.TryGetDecimal(out var m))
            v = m;
        else if (reader.TryGetDouble(out var d))
            v = d;

        if (v is null)
            throw new JsonException("Invalid number");

        reader.Read();
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object? ReadString(ref Utf8JsonReader reader)
    {
        var v = reader.GetString();
        reader.Read();
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object? SkipComment(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.Read();
        return ReadValue(ref reader, options);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureToken(JsonTokenType actual, JsonTokenType expected)
    {
        if (actual != expected)
            throw new JsonException($"Expected: {expected} but actually {actual}.");
    }
}