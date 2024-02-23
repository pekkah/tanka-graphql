using System.Collections.Generic;

namespace Tanka.GraphQL.Server.SourceGenerators;

public record BaseDefinition(
    bool IsClass,
    string Identifier,
    string Namespace,
    string? GraphQLName,
    IReadOnlyList<ObjectPropertyDefinition> Properties,
    IReadOnlyList<ObjectMethodDefinition> Methods);