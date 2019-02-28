using System.Collections.Generic;
using GraphQLParser.AST;
using Newtonsoft.Json;

namespace tanka.graphql
{
    public class Error
    {
        public Error(string message)
        {
            Message = message;
        }

        public string Message { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<GraphQLLocation> Locations { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Extensions { get; set; }
    }
}