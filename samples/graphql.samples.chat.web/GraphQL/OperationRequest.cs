using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace tanka.graphql.samples.chat.web.GraphQL
{
    public class OperationRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public JObject Variables { get; set; }
    }
}