using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

public static class ExecutionResultExtensions
{
    public static void ShouldMatchJson(this ExecutionResult actualResult, string expectedJson)
    {
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));
        if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));

        var actualJson = JToken.FromObject(actualResult,
            JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        var expectedJsonObject = JObject.FromObject(
            JsonConvert.DeserializeObject<ExecutionResult>(expectedJson),
            JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        var jsonEqual = JToken.DeepEquals(expectedJsonObject, actualJson);
        Assert.True(jsonEqual,
            $"Expected: {expectedJsonObject}\r\nActual: {actualJson}");
    }
}