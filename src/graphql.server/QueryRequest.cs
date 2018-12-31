using System.Collections.Generic;
using tanka.graphql.server.utilities;
using Newtonsoft.Json;

namespace tanka.graphql.server
{
    public class QueryRequest
    {
        public string Query { get; set; }

        [JsonConverter(typeof(VariableConverter))]
        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        [JsonConverter(typeof(VariableConverter))]
        public Dictionary<string, object> Extensions { get; set; }
    }
}