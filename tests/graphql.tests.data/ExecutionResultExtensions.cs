using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace fugu.graphql.tests.data
{
    public static class ExecutionResultExtensions
    {
        public static void ShouldMatchJson(this ExecutionResult actualResult, string expectedJson)
        {
            if (expectedJson == null) throw new ArgumentNullException(nameof(expectedJson));
            if (actualResult == null) throw new ArgumentNullException(nameof(actualResult));

            var actualJson = JObject.FromObject(
                actualResult,
                JsonSerializer.CreateDefault(new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));

            var expectedJsonObject = JObject.Parse(expectedJson); 
            Assert.True(JToken.DeepEquals(expectedJsonObject, actualJson), 
                $"Diff: {TestHelpers.Diff(actualJson.ToString(), expectedJson)}");
        }
    }
}