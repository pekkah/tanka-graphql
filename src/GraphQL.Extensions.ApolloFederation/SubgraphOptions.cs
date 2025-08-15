using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Extensions.ApolloFederation;

/// <summary>
/// Apollo Federation v2.3 subgraph configuration options
/// </summary>
public record SubgraphOptions(IReferenceResolversMap ReferenceResolvers)
{
    /// <summary>
    /// Federation specification URL to import. Defaults to Apollo Federation v2.3.
    /// </summary>
    public string FederationSpecUrl { get; init; } = "https://specs.apollo.dev/federation/v2.3";

    /// <summary>
    /// Types and directives to import from the Federation spec. If null, imports all standard Federation types.
    /// </summary>
    public string[]? ImportList { get; init; }
}