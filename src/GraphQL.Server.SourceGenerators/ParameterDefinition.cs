namespace Tanka.GraphQL.Server.SourceGenerators;

public record ParameterDefinition
{
    public string Name { get; init; }

    public string Type { get; init; }

    public bool IsNullable { get; set; } = false;

    public bool? FromServices { get; set; }

    public bool? FromArguments { get; set; }

    public bool IsPrimitive { get; init; }
    public string ClosestMatchingGraphQLTypeName { get; set; }
}