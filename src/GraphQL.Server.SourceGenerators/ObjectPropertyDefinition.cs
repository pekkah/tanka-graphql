﻿namespace Tanka.GraphQL.Server.SourceGenerators;

public record ObjectPropertyDefinition
{
    public string Name { get; init; }

    public string ReturnType { get; init; }

    public bool IsNullable { get; init; }
    public string ClosestMatchingGraphQLTypeName { get; set; }
}