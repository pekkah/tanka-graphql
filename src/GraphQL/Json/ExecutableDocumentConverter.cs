using System.Text.Json;
using System.Text.Json.Serialization;

using Tanka.GraphQL.Language;
using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.Json;

public class ExecutableDocumentConverter : JsonConverter<ExecutableDocument>
{
    public override ExecutableDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //todo: we could use the reader.CopyString to use span
        var text = reader.GetString();
        if (string.IsNullOrEmpty(text))
            return null;

        return Parser.Create(text).ParseExecutableDocument();
    }

    public override void Write(Utf8JsonWriter writer, ExecutableDocument value, JsonSerializerOptions options)
    {
        var text = value.ToString();
        writer.WriteStringValue(text);
    }
}