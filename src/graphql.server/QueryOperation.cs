using System.Collections.Generic;

namespace fugu.graphql.server
{
    public class QueryOperation
    {
        public string Query { get; set; }

        public Dictionary<string, object> Variables { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Extensions { get; set; }
    }
}