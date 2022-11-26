using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tanka.GraphQL.Server.WebSockets.DTOs;
using Xunit;

namespace Tanka.GraphQL.Server.Tests;

public static class OperationMessageExtensions
{
    public static void ShouldMatchJson(this OperationMessage actualResult, string expectedJson)
    {
        if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));
        if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));

        var actualJson = JToken.FromObject(actualResult,
            JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        var expectedJsonObject = JObject.FromObject(
            JsonConvert.DeserializeObject<OperationMessage>(expectedJson),
            JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

        Assert.True(JToken.DeepEquals(expectedJsonObject, actualJson),
            $"Expected: {expectedJsonObject}\r\nActual: {actualJson}");
    }
}