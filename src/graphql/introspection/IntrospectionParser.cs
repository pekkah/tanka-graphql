using System.Text;
using System.Text.Json.Serialization;
using Tanka.GraphQL.DTOs;

namespace Tanka.GraphQL.Introspection
{
    public static class IntrospectionParser
    {
        public static IntrospectionResult Deserialize(string introspectionResult)
        {
            //todo: this is awkward
            var bytes = Encoding.UTF8.GetBytes(introspectionResult);

            var result = DefaultJsonSerializer
                .Serializer
                .Deserialize<IntrospectionExecutionResult>(bytes);

            return new IntrospectionResult
            {
                Schema = result.Data.Schema
            };
        }
    }

    internal class IntrospectionExecutionResult
    {
        [JsonPropertyName("data")]
        public IntrospectionExecutionResultData Data { get; set; }
    }

    internal class IntrospectionExecutionResultData
    {
        [JsonPropertyName("__schema")]
        public __Schema Schema { get; set; }
    }
}