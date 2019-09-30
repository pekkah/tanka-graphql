using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tanka.GraphQL.DTOs
{
    public class QueryRequest
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}