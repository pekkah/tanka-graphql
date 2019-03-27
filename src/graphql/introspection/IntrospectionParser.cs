using Newtonsoft.Json;

namespace tanka.graphql.introspection
{
    public static class IntrospectionParser
    {
        public static IntrospectionResult Deserialize(string introspectionResult)
        {
            var result = JsonConvert.DeserializeObject<IntrospectionExecutionResult>(introspectionResult);

            return new IntrospectionResult
            {
                Schema = result.Data.Schema
            };
        }
    }

    internal class IntrospectionExecutionResult
    {
        [JsonProperty("data")]
        public IntrospectionExecutionResultData Data { get; set; }
    }

    internal class IntrospectionExecutionResultData
    {
        [JsonProperty("__schema")]
        public __Schema Schema { get; set; }
    }
}