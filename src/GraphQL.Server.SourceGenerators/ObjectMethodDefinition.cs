using System.Collections.Generic;

namespace Tanka.GraphQL.Server.SourceGenerators;

public record ObjectMethodDefinition
{
    public string Name { get; set; }

    public bool IsAsync { get; set; }

    public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
        
    public string ReturnType { get; init; }

    public string ClosestMatchingGraphQLTypeName { get; set; }

    public bool IsStatic { get; set; }
}