using System.Collections.Generic;
using GraphQLParser.AST;

namespace Tanka.GraphQL
{
    public class ExecutionError
    {
        public ExecutionError(string message)
        {
            Message = message;
        }

        public string Message { get; }

        public List<GraphQLLocation> Locations { get; set; }

        public List<object> Path { get; set; }

        public Dictionary<string, object> Extensions { get; set; }

        public void Extend(string key, object value)
        {
            if (Extensions == null)
                Extensions = new Dictionary<string, object>();

            Extensions[key] = value;
        }
    }
}