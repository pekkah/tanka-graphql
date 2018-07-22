using System.Collections.Generic;

namespace fugu.graphql.samples.chat.web.GraphQL
{
    public class OperationRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }
    }
}