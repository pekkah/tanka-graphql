using System.Collections.Generic;
using Tanka.GraphQL.Server.WebSockets.DTOs;

namespace Tanka.GraphQL.Server.WebSockets
{
    public static class Payloads
    {
        public static Dictionary<string, object> ToErrors(params ExecutionError[] errors)
        {
            var result = new Dictionary<string, object> {{"errors", errors}};

            return result;
        }

        public static Dictionary<string, object> ToData(ExecutionResult result)
        {
            return new Dictionary<string, object>
            {
                ["data"] = result.Data,
                ["errors"] = result.Errors,
                ["extensions"] = result.Extensions
            };
        }

        public static OperationMessageQueryPayload GetQuery(Dictionary<string, object> payload)
        {
            return new OperationMessageQueryPayload
            {
                OperationName = (string) payload["operationName"],
                Query = (string) payload["query"],
                Variables = payload.ContainsKey("variables")
                    ? payload["variables"] as Dictionary<string, object>
                    : null,
                Extensions = payload.ContainsKey("extensions")
                    ? payload["extensions"] as Dictionary<string, object>
                    : null
            };
        }

        public static Dictionary<string, object> ToQuery(OperationMessageQueryPayload query)
        {
            return new Dictionary<string, object>()
            {
                ["operationName"] = query.OperationName,
                ["query"] = query.Query,
                ["variables"] = query.Variables,
                ["extensions"] = query.Extensions
            };
        }

        public static ExecutionResult GetResult(Dictionary<string, object> result)
        {
            return new ExecutionResult()
            {
                Data = result.ContainsKey("data")
                    ? result["data"] as Dictionary<string, object>
                    : null,
                Errors = GetErrors(result),
                Extensions = result.ContainsKey("extensions")
                    ? result["extensions"] as Dictionary<string, object>
                    : null
            };
        }

        private static List<ExecutionError> GetErrors(Dictionary<string, object> executionResult)
        {
            if (!executionResult.ContainsKey("errors"))
                return null;

            var errorsList = executionResult["errors"] as IEnumerable<object>;

            var result = new List<ExecutionError>();
            foreach (var errorObject in errorsList)
            {
                var errorDictionary = errorObject as Dictionary<string, object>;

                result.Add(new ExecutionError(errorDictionary["message"].ToString())
                {
                    Extensions = errorDictionary.ContainsKey("extensions")
                        ? errorDictionary["extensions"] as Dictionary<string, object>
                        : null,
                });
            }

            return result;
        }
    }
}