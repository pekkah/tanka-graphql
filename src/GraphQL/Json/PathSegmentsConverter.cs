using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.Json;

public class PathSegmentsConverter : JsonConverter<object[]>
{
    public override object[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        var items = new List<object>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.String)
            {
                items.Add(reader.GetString() ?? string.Empty);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                items.Add(reader.GetInt32());
            }
        }

        return items.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            if (item is string s)
                writer.WriteStringValue(s);
            else if (item is int i)
            {
                writer.WriteNumberValue(i);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
        writer.WriteEndArray();
    }
}