using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.DTOs
{
    public class ObjectDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadDictionary(doc.RootElement, options);
        }


        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value,
            JsonSerializerOptions options)
        {
            WriteDictionary(writer, value, options);
        }

        private Dictionary<string, object> ReadDictionary(JsonElement element, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, object>();
            foreach (var property in element.EnumerateObject())
            {
                var key = property.Name;
                var value = property.Value;
                object resultValue = null;

                switch (value.ValueKind)
                {
                    case JsonValueKind.Object:
                        resultValue = ReadDictionary(value, options);
                        break;
                    case JsonValueKind.Number:
                        if (value.TryGetInt32(out var i))
                            resultValue = i;
                        else if (value.TryGetDouble(out var d))
                            resultValue = d;
                        else if (value.TryGetDecimal(out var dd))
                            resultValue = dd;
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        resultValue = value.GetBoolean();
                        break;
                    case JsonValueKind.String:
                        resultValue = value.GetString();
                        break;
                    case JsonValueKind.Null:
                        // default value is null
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected value kind: {value.ValueKind}");
                }

                result[key] = resultValue;
            }

            return result;
        }

        private void WriteDictionary(Utf8JsonWriter writer, Dictionary<string, object> dictionary,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var entry in dictionary)
            {
                var value = entry.Value;

                if (value == null)
                {
                    writer.WriteNull(entry.Key);
                    continue;
                }

                switch (value)
                {
                    case int intValue:
                        writer.WriteNumber(entry.Key, intValue);
                        break;
                    case double doubleValue:
                        writer.WriteNumber(entry.Key, doubleValue);
                        break;
                    case decimal decimalValue:
                        writer.WriteNumber(entry.Key, decimalValue);
                        break;
                    case string stringValue:
                        writer.WriteString(entry.Key, stringValue);
                        break;
                    case bool boolValue:
                        writer.WriteBoolean(entry.Key, boolValue);
                        break;
                    case Dictionary<string, object> subDictionary:
                        writer.WritePropertyName(entry.Key);
                        WriteDictionary(writer, subDictionary, options);
                        break;
                }
            }

            writer.WriteEndObject();
        }
    }
}