using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.Json;

/// <summary>
/// JSON converter for NodePath objects
/// </summary>
public class PathConverter : JsonConverter<NodePath>
{
    public override NodePath? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var path = new NodePath();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.String)
            {
                path.Append(reader.GetString() ?? string.Empty);
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                path.Append(reader.GetInt32());
            }
        }

        return path;
    }

    public override void Write(Utf8JsonWriter writer, NodePath value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var segment in value.Segments)
        {
            if (segment is string s)
                writer.WriteStringValue(s);
            else if (segment is int i)
                writer.WriteNumberValue(i);
            else
                writer.WriteNullValue();
        }
        writer.WriteEndArray();
    }
}