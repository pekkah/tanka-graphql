using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.Server.Links.DTOs
{
    public class ObjectDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadDictionary(doc.RootElement, options);
        }

        private void EnsureTokenType(JsonTokenType actual, JsonTokenType expected)
        {
            if (actual != expected)
                throw new InvalidOperationException(
                    $"Unexpected token type '{actual}' expected '{expected}'");
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

            if (!result.Any())
                return null;

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

                WriteProperty(writer, options, value, entry.Key);
            }

            writer.WriteEndObject();
        }

        private void WriteProperty(Utf8JsonWriter writer, JsonSerializerOptions options, object value, string key)
        {
            switch (value)
            {
                case int intValue:
                    writer.WriteNumber(key, intValue);
                    break;
                case double doubleValue:
                    writer.WriteNumber(key, doubleValue);
                    break;
                case decimal decimalValue:
                    writer.WriteNumber(key, decimalValue);
                    break;
                case string stringValue:
                    writer.WriteString(key, stringValue);
                    break;
                case bool boolValue:
                    writer.WriteBoolean(key, boolValue);
                    break;
                case Dictionary<string, object> subDictionary:
                    writer.WritePropertyName(key);
                    WriteDictionary(writer, subDictionary, options);
                    break;
                case IEnumerable list:
                    writer.WritePropertyName(key);
                    WriteArray(writer, list, options);
                    break;
            }
        }

        private void WriteArray(Utf8JsonWriter writer, IEnumerable list, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var value in list)
            {
                WriteArrayValue(writer, value, options);
            }

            writer.WriteEndArray();
        }

        private void WriteArrayValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case decimal decimalValue:
                    writer.WriteNumberValue(decimalValue);
                    break;
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;
                case Dictionary<string, object> subDictionary:
                    WriteDictionary(writer, subDictionary, options);
                    break;
                case IEnumerable list:
                    WriteArray(writer, list, options);
                    break;
            }
        }
    }
}