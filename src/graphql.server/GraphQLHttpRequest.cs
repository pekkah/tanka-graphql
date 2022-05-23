using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tanka.GraphQL.Server.Links.DTOs;

namespace Tanka.GraphQL.Server;

public class GraphQLHttpRequest
{
    public string? OperationName { get; set; }

    public string Query { get; set; } = string.Empty;

    [JsonConverter(typeof(ObjectDictionaryConverter))]
    public Dictionary<string, object?>? Variables { get; set; }
}