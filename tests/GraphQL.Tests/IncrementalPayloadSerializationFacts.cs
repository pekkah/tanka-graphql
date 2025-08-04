using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Tanka.GraphQL.Language.Nodes;

using Xunit;

namespace Tanka.GraphQL.Tests;

public class IncrementalPayloadSerializationFacts
{
    [Fact]
    public void IncrementalPayload_Should_Serialize_Path_As_Array()
    {
        // Given
        var path = new NodePath().Append("user").Append("profile");
        var payload = new IncrementalPayload
        {
            Label = "test-label",
            Path = path,
            Data = new Dictionary<string, object?> { ["field"] = "value" }
        };

        // When
        var json = JsonSerializer.Serialize(payload);
        var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

        // Then
        Assert.True(deserialized.TryGetProperty("label", out var labelElement));
        Assert.Equal("test-label", labelElement.GetString());

        Assert.True(deserialized.TryGetProperty("path", out var pathElement));
        Assert.Equal(JsonValueKind.Array, pathElement.ValueKind);

        var pathArray = pathElement.EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(new[] { "user", "profile" }, pathArray);

        Assert.True(deserialized.TryGetProperty("data", out var dataElement));
        Assert.True(dataElement.TryGetProperty("field", out var fieldElement));
        Assert.Equal("value", fieldElement.GetString());
    }

    [Fact]
    public void IncrementalPayload_Should_Serialize_Null_Path_As_Null()
    {
        // Given
        var payload = new IncrementalPayload
        {
            Label = "test-label",
            Path = null,
            Data = new Dictionary<string, object?> { ["field"] = "value" }
        };

        // When
        var json = JsonSerializer.Serialize(payload);
        var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

        // Then
        Assert.True(deserialized.TryGetProperty("label", out var labelElement));
        Assert.Equal("test-label", labelElement.GetString());

        // Path should not be present when null (due to JsonIgnore(WhenWritingNull))
        Assert.False(deserialized.TryGetProperty("path", out _));
    }

    [Fact]
    public void IncrementalPayload_Should_Omit_Label_When_Null()
    {
        // Given
        var path = new NodePath().Append("user");
        var payload = new IncrementalPayload
        {
            Label = null,
            Path = path,
            Data = new Dictionary<string, object?> { ["field"] = "value" }
        };

        // When
        var json = JsonSerializer.Serialize(payload);
        var deserialized = JsonSerializer.Deserialize<JsonElement>(json);

        // Then
        // Label should not be present when null (due to JsonIgnore(WhenWritingNull))
        Assert.False(deserialized.TryGetProperty("label", out _));

        Assert.True(deserialized.TryGetProperty("path", out var pathElement));
        Assert.Equal(JsonValueKind.Array, pathElement.ValueKind);

        var pathArray = pathElement.EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(new[] { "user" }, pathArray);
    }
}