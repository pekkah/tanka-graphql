using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.Serialization.Converters
{
    public class OperationMessageConverter : JsonConverter<OperationMessage>
    {
        public override OperationMessage Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            EnsureTokenType(reader.TokenType, JsonTokenType.StartObject);

            // read start of the object
            reader.Read();

            var type = PeekPayloadType(reader);

            var result = new OperationMessage();
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "id":
                        result.Id = reader.GetString();
                        reader.Read();
                        break;
                    case "type":
                        result.Type = type;
                        reader.Read();
                        break;
                    case "payload":
                        result.Payload = ReadPayload(ref reader, type, options);
                        break;
                }
            }
            
            EnsureTokenType(reader.TokenType, JsonTokenType.EndObject);
            reader.Read();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, OperationMessage value, JsonSerializerOptions options)
        {
        }

        private object ReadPayload(ref Utf8JsonReader reader, string payloadType, JsonSerializerOptions options)
        {
            return payloadType switch
            {
                MessageType.GQL_CONNECTION_INIT => ReadConnectionParams(ref reader, options),
                MessageType.GQL_START => ReadQuery(ref reader, options),
                _ => ReadNull(ref reader, options)
            };
        }

        private object ReadNull(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.Read();
            return null;
        }

        private object ReadConnectionParams(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
        }

        private object ReadQuery(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<OperationMessageQueryPayload>(ref reader, options);
        }

        private string PeekPayloadType(Utf8JsonReader reader)
        {
            string type = null;
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read(); //skip name

                if (propertyName == "type")
                {
                    type = reader.GetString();
                    break;
                }

                reader.Read(); // skip value
            }

            return type;
        }

        private void EnsureTokenType(JsonTokenType actual, JsonTokenType expected)
        {
            if (actual != expected)
                throw new InvalidOperationException(
                    $"Unexpected token type '{actual}' expected '{expected}'");
        }
    }
}