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
            var result = ReadDictionary(ref reader, options);

            return result;
        }

        private Dictionary<string, object> ReadDictionary(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}");

            reader.Read();

            var result = new Dictionary<string, object>();
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}");

                // key
                var key = reader.GetString();
                reader.Read();

                // value
                object value = null;
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        value = ReadDictionary(ref reader, options);
                        break;
                    case JsonTokenType.Number:
                        if (reader.TryGetInt32(out var i))
                            value = i;
                        else if (reader.TryGetDouble(out var d))
                            value = d;
                        else if (reader.TryGetDecimal(out var dd))
                            value = dd;

                        reader.Read();
                        break;
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        value = reader.GetBoolean();
                        reader.Read();
                        break;
                    case JsonTokenType.String:
                        value = reader.GetString();
                        reader.Read();
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}");
                        break;

                }

                result[key] = value;
            }

            if (reader.TokenType != JsonTokenType.EndObject)
                throw new InvalidOperationException($"Unexpected token type: {reader.TokenType}");

            reader.Read();
            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value,
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}