using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public class GraphQLRequestContext
{
    public IFeatureCollection Features { get; } = new FeatureCollection();
    
    public string? OperationName { get; set; }
    
    public string Query { get; set; } = string.Empty;

    public Dictionary<string, object?>? Variables { get; set; }
    
    public ISchema? Schema { get; set; }
}