using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tanka.GraphQL.DTOs
{
    public class QueryRequest
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonConverter(typeof(NestedDictionaryConverter))]
        [JsonProperty("variables")]
        public Dictionary<string, object> Variables { get; set; }

        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [JsonConverter(typeof(NestedDictionaryConverter))]
        [JsonProperty("extensions")]
        public Dictionary<string, object> Extensions { get; set; }
    }
}