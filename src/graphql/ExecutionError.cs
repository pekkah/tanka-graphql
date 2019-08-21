using System.Collections.Generic;
using GraphQLParser.AST;
using Newtonsoft.Json;

namespace tanka.graphql
{
    public class ExecutionError
    {
        public ExecutionError(string message)
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

        public void Extend(string key, object value)
        {
            if (Extensions == null)
                Extensions = new Dictionary<string, object>();

            Extensions[key] = value;
        }
    }
}