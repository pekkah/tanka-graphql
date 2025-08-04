using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Xunit;

namespace Tanka.GraphQL.Tests;

public static class ExecutionResultExtensions
{
    public static void ShouldMatchJson(this ExecutionResult actualResult, [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson)
    {
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));
        if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));

        var actualJson = JToken.FromObject(actualResult,
            Newtonsoft.Json.JsonSerializer.Create(new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        var expectedJsonObject = JObject.FromObject(
            JsonConvert.DeserializeObject<ExecutionResult>(expectedJson),
            Newtonsoft.Json.JsonSerializer.Create(new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        var jsonEqual = JToken.DeepEquals(expectedJsonObject, actualJson);
        Assert.True(jsonEqual,
            $"Expected: {expectedJsonObject}\r\nActual: {actualJson}");
    }

    /// <summary>
    /// Verifies that an ExecutionResult with incremental delivery matches the expected JSON structure
    /// </summary>
    public static void ShouldMatchIncrementalJson(this ExecutionResult actualResult,
        [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson)
    {
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));
        if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));

        // Verify the result has incremental fields
        Assert.True(actualResult.HasNext.HasValue || (actualResult.Incremental?.Any() ?? false),
            "ExecutionResult should have HasNext or Incremental fields for incremental delivery");

        // Use the same JSON comparison as regular ShouldMatchJson
        actualResult.ShouldMatchJson(expectedJson);
    }

    /// <summary>
    /// Collects all results from an IAsyncEnumerable&lt;ExecutionResult&gt; stream and verifies them against expected JSON
    /// </summary>
    public static async Task ShouldMatchStreamJson(this IAsyncEnumerable<ExecutionResult> stream,
        [StringSyntax(StringSyntaxAttribute.Json)] string expectedJson)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        // Create a combined result object for comparison
        var combinedResult = new
        {
            results = results.ToArray()
        };

        // Use System.Text.Json to properly serialize ExecutionResult with its converters
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var actualJsonString = System.Text.Json.JsonSerializer.Serialize(combinedResult, options);
        var actualJson = JToken.Parse(actualJsonString);

        var expectedJsonObject = JToken.Parse(expectedJson);
        var jsonEqual = JToken.DeepEquals(expectedJsonObject, actualJson);

        Assert.True(jsonEqual,
            $"Expected: {expectedJsonObject}\r\nActual: {actualJson}");
    }

    /// <summary>
    /// Collects all results from a stream and formats them as a readable string for debugging
    /// </summary>
    public static async Task<string> ToDebugString(this IAsyncEnumerable<ExecutionResult> stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var results = new List<ExecutionResult>();
        await foreach (var result in stream)
        {
            results.Add(result);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Stream contained {results.Count} result(s):");

        for (int i = 0; i < results.Count; i++)
        {
            sb.AppendLine($"Result {i + 1}:");
            var json = JsonConvert.SerializeObject(results[i], Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            sb.AppendLine(json);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}