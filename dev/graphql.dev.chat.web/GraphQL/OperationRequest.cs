using Newtonsoft.Json.Linq;

namespace Tanka.GraphQL.Samples.Chat.Web.GraphQL
{
    public class OperationRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public JObject Variables { get; set; }
    }
}