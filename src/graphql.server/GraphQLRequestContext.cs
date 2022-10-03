using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;
using Tanka.GraphQL.Language.Nodes;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Server;

public class GraphQLRequestContext
{
    public IFeatureCollection Features { get; } = new FeatureCollection();
    
    public string? OperationName { get; set; }
    
    public string Query { get; set; } = string.Empty;

    public Dictionary<string, object?>? Variables { get; set; }
    
    public ISchema? Schema { get; set; }

    public IServiceProvider? RequestServices { get; set; }

    public ExecutableDocument? Document { get; set; }
    
    public OperationDefinition? Operation { get; set; }

    public IReadOnlyDictionary<string, object?>? CoercedVariables { get; set; }
}