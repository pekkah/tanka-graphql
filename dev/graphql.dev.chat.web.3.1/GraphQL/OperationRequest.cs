using System.Collections.Generic;

namespace Tanka.GraphQL.Samples.Chat.Web.GraphQL
{
    public class OperationRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }
    }
}