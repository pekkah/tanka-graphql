using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Tanka.GraphQL.Json;
using Xunit;

namespace Tanka.GraphQL.Tests;

public class ExecutionResultFacts
{
    [Fact]
    public void ExecutionResult_serializes_basic_response()
    {
        // Given
        var result = new ExecutionResult
        {
            Data = new Dictionary<string, object?>
            {
                ["field1"] = "value1",
                ["field2"] = 42
            }
        };

        // When
        var json = JsonSerializer.Serialize(result);

        // Then
        var expected = """{"data":{"field1":"value1","field2":42}}""";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void ExecutionResult_serializes_with_errors()
    {
        // Given
        var result = new ExecutionResult
        {
            Data = new Dictionary<string, object?> { ["field1"] = "value1" },
            Errors = new[]
            {
                new ExecutionError { Message = "Test error" }
            }
        };

        // When
        var json = JsonSerializer.Serialize(result);

        // Then
        Assert.Contains("\"errors\"", json);
        Assert.Contains("Test error", json);
    }

    [Fact]
    public void ExecutionResult_serializes_with_hasNext()
    {
        // Given
        var result = new ExecutionResult
        {
            Data = new Dictionary<string, object?> { ["field1"] = "value1" },
            HasNext = true
        };

        // When
        var json = JsonSerializer.Serialize(result);

        // Then
        Assert.Contains("\"hasNext\":true", json);
    }

    [Fact]
    public void ExecutionResult_serializes_with_incremental_payloads()
    {
        // Given
        var path = new NodePath();
        path.Append("user").Append("profile");

        var result = new ExecutionResult
        {
            Data = new Dictionary<string, object?> { ["field1"] = "value1" },
            HasNext = true,
            Incremental = new[]
            {
                new IncrementalPayload
                {
                    Label = "deferredProfile",
                    Path = path,
                    Data = new Dictionary<string, object?>
                    {
                        ["name"] = "John Doe",
                        ["email"] = "john@example.com"
                    }
                }
            }
        };

        // When
        var json = JsonSerializer.Serialize(result);

        // Then
        Assert.Contains("\"hasNext\":true", json);
        Assert.Contains("\"incremental\"", json);
        Assert.Contains("\"label\":\"deferredProfile\"", json);
        Assert.Contains("\"path\":[\"user\",\"profile\"]", json);
        Assert.Contains("\"name\":\"John Doe\"", json);
    }

    [Fact]
    public void ExecutionResult_omits_null_fields()
    {
        // Given
        var result = new ExecutionResult
        {
            Data = new Dictionary<string, object?> { ["field1"] = "value1" }
            // HasNext and Incremental are null
        };

        // When
        var json = JsonSerializer.Serialize(result);

        // Then
        Assert.DoesNotContain("\"hasNext\"", json);
        Assert.DoesNotContain("\"incremental\"", json);
    }

    [Fact]
    public void IncrementalPayload_serializes_with_errors()
    {
        // Given
        var payload = new IncrementalPayload
        {
            Label = "erroredFragment",
            Path = new NodePath().Append("field"),
            Data = null,
            Errors = new[]
            {
                new ExecutionError { Message = "Fragment error" }
            }
        };

        // When
        var json = JsonSerializer.Serialize(payload);

        // Then
        Assert.Contains("\"label\":\"erroredFragment\"", json);
        Assert.Contains("\"path\":[\"field\"]", json);
        Assert.Contains("\"errors\"", json);
        Assert.Contains("Fragment error", json);
    }

    [Fact]
    public void PathConverter_serializes_complex_path()
    {
        // Given
        var path = new NodePath();
        path.Append("users").Append(0).Append("profile").Append("addresses").Append(1).Append("street");

        // When
        var json = JsonSerializer.Serialize(path, new JsonSerializerOptions 
        { 
            Converters = { new PathConverter() } 
        });

        // Then
        var expected = """["users",0,"profile","addresses",1,"street"]""";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void PathConverter_deserializes_complex_path()
    {
        // Given
        var json = """["users",0,"profile","addresses",1,"street"]""";

        // When
        var path = JsonSerializer.Deserialize<NodePath>(json, new JsonSerializerOptions 
        { 
            Converters = { new PathConverter() } 
        });

        // Then
        Assert.NotNull(path);
        var segments = path.Segments.ToArray();
        Assert.Equal(6, segments.Length);
        Assert.Equal("users", segments[0]);
        Assert.Equal(0, segments[1]);
        Assert.Equal("profile", segments[2]);
        Assert.Equal("addresses", segments[3]);
        Assert.Equal(1, segments[4]);
        Assert.Equal("street", segments[5]);
    }
}