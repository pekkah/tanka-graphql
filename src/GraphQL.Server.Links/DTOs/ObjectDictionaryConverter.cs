using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.Server.Links.DTOs;

public class ObjectDictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;

        return ReadObject(ref element, options);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }

    private Dictionary<string, object> ReadObject(ref JsonElement element, JsonSerializerOptions options)
    {
        var result = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject())
        {
            var name = property.Name;
            var value = property.Value;
            var convertedValue = ReadValue(ref value, options);
            result[name] = convertedValue;
        }

        return result;
    }

    private object ReadValue(ref JsonElement element, JsonSerializerOptions options)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return null;

        if (element.ValueKind == JsonValueKind.Object) return ReadObject(ref element, options);

        if (element.ValueKind == JsonValueKind.Array)
        {
            var items = new List<object>();
            foreach (var arrayElement in element.EnumerateArray())
            {
                var v = arrayElement;
                var arrayValue = ReadValue(ref v, options);
                items.Add(arrayValue);
            }

            return items;
        }

        object value;
        switch (element.ValueKind)
        {
            case JsonValueKind.True:
                value = true;
                break;
            case JsonValueKind.False:
                value = false;
                break;
            case JsonValueKind.Number:
            {
                if (element.TryGetInt32(out var intValue))
                    value = intValue;
                else if (element.TryGetInt64(out var longValue))
                    value = longValue;
                else
                    value = element.GetDouble();

                break;
            }
            case JsonValueKind.String:
                value = element.GetString();
                break;
            default:
                value = element.GetRawText();
                break;
        }

        return value;
    }
}