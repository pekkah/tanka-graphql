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
                OperationName = payload.GetValue<string>("operationName"),
                Query = payload.GetValue<string>("query"),
                Variables = payload.GetValue<Dictionary<string, object>>("variables"),
                Extensions = payload.GetValue<Dictionary<string, object>>("extensions")
            };
        }

        public static Dictionary<string, object> ToQuery(OperationMessageQueryPayload query)
        {
            return new Dictionary<string, object>
            {
                ["operationName"] = query.OperationName,
                ["query"] = query.Query,
                ["variables"] = query.Variables,
                ["extensions"] = query.Extensions
            };
        }

        public static ExecutionResult GetResult(Dictionary<string, object> result)
        {
            return new ExecutionResult
            {
                Data = result.GetValue<Dictionary<string, object>>("data"),
                Errors = GetErrors(result),
                Extensions = result.GetValue<Dictionary<string, object>>("extensions")
            };
        }

        private static List<ExecutionError> GetErrors(Dictionary<string, object> executionResult)
        {
            if (!executionResult.TryGetValue("errors", out var errorsObject))
                return null;

            if (errorsObject == null)
                return null;

            var errorsList = errorsObject as IEnumerable<object>;

            var result = new List<ExecutionError>();
            foreach (var errorObject in errorsList)
            {
                var errorDictionary = errorObject as Dictionary<string, object>;

                result.Add(new ExecutionError(errorDictionary["message"].ToString())
                {
                    Extensions = errorDictionary.ContainsKey("extensions")
                        ? errorDictionary["extensions"] as Dictionary<string, object>
                        : null
                });
            }

            return result;
        }
    }
}