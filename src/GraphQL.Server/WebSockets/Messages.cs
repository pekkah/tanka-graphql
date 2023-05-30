using System.Text.Json;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Json;

namespace Tanka.GraphQL.Server.WebSockets;

[JsonConverter(typeof(MessageConverter))]
public abstract class MessageBase
{
    [JsonPropertyName("type")] 
    public string Type { get; set; } = null!;

    public override string ToString()
    {
        return $"Message({Type})";
    }
}

public class MessageConverter: JsonConverter<MessageBase>
{
    public override MessageBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var messageType = ReadMessageType(reader);

        //todo: we could create a code generator for this converter
        return messageType switch
        {
            MessageTypes.ConnectionInit => JsonSerializer.Deserialize<ConnectionInit>(ref reader, options),
            MessageTypes.Ping => JsonSerializer.Deserialize<Ping>(ref reader, options),
            MessageTypes.Subscribe => JsonSerializer.Deserialize<Subscribe>(ref reader, options),
            MessageTypes.Complete => JsonSerializer.Deserialize<Complete>(ref reader, options),
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
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

public static class MessageTypes
{
    public const string ConnectionInit = "connection_init";
    public const string ConnectionAck = "connection_ack";
    public const string Ping = "ping";
    public const string Pong = "pong";
    public const string Subscribe = "subscribe";
    public const string Next = "next";
    public const string Error = "error";
    public const string Complete = "complete";
}

public class ConnectionInit : MessageBase
{
    public ConnectionInit()
    {
        Type = MessageTypes.ConnectionInit;
    }

    [JsonPropertyName("payload")]
    [JsonConverter(typeof(NestedDictionaryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, object>? Payload { get; set; }
}

public class Subscribe: MessageBase
{
    public Subscribe()
    {
        Type = MessageTypes.Subscribe;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("payload")]
    public GraphQLHttpRequest Payload { get; set; }

    public override string ToString()
    {
        return $"Subscribe: {Id}";
    }
}

public class Next: MessageBase
{
    public Next()
    {
        Type = MessageTypes.Next;
    }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("payload")]
    public required ExecutionResult Payload { get; set; }
}

public class Error : MessageBase
{
    public Error()
    {
        Type = MessageTypes.Error;
    }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("payload")]
    public required ExecutionError[] Payload { get; set; }
}

public class Complete: MessageBase
{
    public Complete()
    {
        Type = MessageTypes.Complete;
    }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    public override string ToString()
    {
        return $"Complete: {Id}";
    }
}