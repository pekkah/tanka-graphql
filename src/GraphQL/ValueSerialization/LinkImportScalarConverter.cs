using System.Text.Json;

using Tanka.GraphQL.Language.Nodes;

namespace Tanka.GraphQL.ValueSerialization;

/// <summary>
/// Converter for link__Import scalar type that handles import specifications from @link directive.
/// Supports both simple string imports and aliased imports as JSON objects.
/// </summary>
public class LinkImportScalarConverter : IValueConverter
{
    public object? ParseLiteral(ValueBase input) => input switch
    {
        StringValue stringValue => stringValue.ToString(),
        ObjectValue objectValue => ParseImportObject(objectValue),
        NullValue => null,
        _ => throw new InvalidOperationException(
            $"Invalid literal type for link__Import: {input.GetType().Name}. Expected StringValue or ObjectValue.")
    };

    public object? ParseValue(object? input) => input switch
    {
        string stringValue => stringValue,
        JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String => jsonElement.GetString(),
        JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Object => ParseJsonImportObject(jsonElement),
        _ when input is null => null,
        _ => throw new InvalidOperationException(
            $"Invalid value type for link__Import: {input.GetType().Name}. Expected string or object.")
    };

    public object? Serialize(object? value) => value switch
    {
        string stringValue => stringValue,
        LinkImport linkImport => SerializeLinkImport(linkImport),
        _ when value is null => null,
        _ => throw new InvalidOperationException(
            $"Cannot serialize value of type {value.GetType().Name} as link__Import")
    };

    public ValueBase SerializeLiteral(object? value) => value switch
    {
        string stringValue => (StringValue)stringValue,
        LinkImport linkImport => SerializeLinkImportLiteral(linkImport),
        _ when value is null => new NullValue(),
        _ => throw new InvalidOperationException(
            $"Cannot serialize literal value of type {value.GetType().Name} as link__Import")
    };

    private static LinkImport ParseImportObject(ObjectValue objectValue)
    {
        string? name = null;
        string? alias = null;

        foreach (var field in objectValue)
        {
            var fieldName = field.Name.ToString();
            switch (fieldName)
            {
                case "name":
                    if (field.Value is StringValue nameValue)
                        name = nameValue.ToString();
                    break;
                case "as":
                    if (field.Value is StringValue aliasValue)
                        alias = aliasValue.ToString();
                    break;
            }
        }

        if (name is null)
            throw new InvalidOperationException("link__Import object must have 'name' field");

        return new LinkImport(name, alias);
    }

    private static LinkImport ParseJsonImportObject(JsonElement jsonElement)
    {
        if (!jsonElement.TryGetProperty("name", out var nameElement))
            throw new InvalidOperationException("link__Import object must have 'name' property");

        var name = nameElement.GetString()
            ?? throw new InvalidOperationException("link__Import 'name' must be a string");

        string? alias = null;
        if (jsonElement.TryGetProperty("as", out var aliasElement))
        {
            alias = aliasElement.GetString();
        }

        return new LinkImport(name, alias);
    }

    private static object SerializeLinkImport(LinkImport linkImport)
    {
        if (linkImport.Alias is null)
        {
            return linkImport.Name;
        }

        return new Dictionary<string, object>
        {
            ["name"] = linkImport.Name,
            ["as"] = linkImport.Alias
        };
    }

    private static ValueBase SerializeLinkImportLiteral(LinkImport linkImport)
    {
        if (linkImport.Alias is null)
        {
            return (StringValue)linkImport.Name;
        }

        return new ObjectValue(new ObjectField[]
        {
            new(new Name("name"), (StringValue)linkImport.Name),
            new(new Name("as"), (StringValue)linkImport.Alias)
        });
    }
}

/// <summary>
/// Represents an import specification for @link directive
/// </summary>
public record LinkImport(string Name, string? Alias = null);