using System.Text.Json;

namespace Tanka.GraphQL.Server.SourceGenerators;

public record ObjectPropertyDefinition
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public required string ClosestMatchingGraphQLTypeName { get; set; }
    
    public InputTypeDefinition? ReturnTypeObject { get; set; }

    public string AsField => $"{JsonNamingPolicy.CamelCase.ConvertName(Name)}: {ClosestMatchingGraphQLTypeName}";

    public string ResolverName => $"{Name}";

    public bool IsStatic { get; set; }
}