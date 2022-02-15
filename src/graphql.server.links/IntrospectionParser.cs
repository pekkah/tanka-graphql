using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Introspection;

namespace Tanka.GraphQL.Server.Links;

public static class IntrospectionParser
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new DirectiveLocationConverter(),
            new TypeKindConverter()
        }
    };

    public static IntrospectionResult Deserialize(
        string introspectionResult)
    {
        //todo: this is awkward
        var bytes = Encoding.UTF8.GetBytes(introspectionResult);

        var result = JsonSerializer
            .Deserialize<IntrospectionExecutionResult>(bytes, _jsonOptions);

        return new IntrospectionResult
        {
            Schema = result.Data.Schema
        };
    }
}

internal class IntrospectionExecutionResult
{
    [JsonPropertyName("data")] public IntrospectionExecutionResultData Data { get; set; }
}

internal class IntrospectionExecutionResultData
{
    [JsonPropertyName("__schema")] public __Schema Schema { get; set; }
}

internal class DirectiveLocationConverter : JsonConverter<__DirectiveLocation>
{
    public override __DirectiveLocation Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidOperationException(
                $"Unexpected token type '{reader.TokenType}'. Expected {JsonTokenType.String}");

        var value = reader.GetString();
        //reader.Read();

        var enumValue = (__DirectiveLocation)Enum.Parse(typeof(__DirectiveLocation), value, true);

        return enumValue;
    }

    public override void Write(Utf8JsonWriter writer, __DirectiveLocation value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

internal class TypeKindConverter : JsonConverter<__TypeKind>
{
    public override __TypeKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidOperationException(
                $"Unexpected token type '{reader.TokenType}'. Expected {JsonTokenType.String}");

        var value = reader.GetString();
        //reader.Read();

        var enumValue = (__TypeKind)Enum.Parse(typeof(__TypeKind), value, true);

        return enumValue;
    }

    public override void Write(Utf8JsonWriter writer, __TypeKind value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}