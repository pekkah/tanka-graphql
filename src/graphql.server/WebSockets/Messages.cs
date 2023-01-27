using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.Server.WebSockets;

[JsonConverter(typeof(MessageConverter))]
public abstract class MessageBase
{
    [JsonPropertyName("type")] 
    public string Type { get; set; } = null!;
}

public class MessageConverter: JsonConverter<MessageBase>
{
    public override MessageBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var messageType = ReadMessageType(reader);

        return messageType switch
        {
            MessageTypes.ConnectionInit => JsonSerializer.Deserialize<ConnectionInit>(ref reader, options),
            _ => throw new JsonException()
        };
    }

    private string? ReadMessageType(Utf8JsonReader reader)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                if (propertyName == "type")
                {
                    reader.Read();
                    return reader.GetString();
                }
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, MessageBase value, JsonSerializerOptions options)
    {
    }
}

public static class MessageTypes
{
    public const string ConnectionInit = "connection_init";
    public const string ConnectionAck = "connection_ack";
    public const string Ping = "ping";
    public const string Pong = "pong";
}

public class ConnectionInit : MessageBase
{
    public ConnectionInit()
    {
        Type = MessageTypes.ConnectionInit;
    }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object>? Payload { get; set; }
}

public class ConnectionAck : MessageBase
{
    public ConnectionAck()
    {
        Type = MessageTypes.ConnectionAck;
    }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object>? Payload { get; set; }
}

public class Ping : MessageBase
{
    public Ping()
    {
        Type = MessageTypes.Ping;
    }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object>? Payload { get; set; }
}

public class Pong : MessageBase
{
    public Pong()
    {
        Type = MessageTypes.Pong;
    }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    public IReadOnlyDictionary<string, object>? Payload { get; set; }
}