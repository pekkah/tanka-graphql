namespace GraphQL.Samples.ApolloFederation.Compatibility;

public record CaseStudy(string CaseNumber, string? Description);

public record DeprecatedProduct(string Sku, string Package, string? Reason, User? CreatedBy);

public record Inventory(string Id, DeprecatedProduct[] DeprecatedProducts);

public record Product(
    string Id,
    string? Sku,
    string? Package,
    ProductVariation? Variation,
    ProductDimension? Dimensions,
    User? CreatedBy,
    string? Notes,
    ProductResearch[] Research);

public record ProductDimension(string? Size, double? Weight, string? Unit);

public record ProductResearch(CaseStudy Study, string? Outcome);

public record ProductVariation(string Id);

public record User(string Email, int? TotalProductsCreated);

public class Data
{
    public static readonly CaseStudy[] CaseStudies =
    [
        new("1234", "Federation study"),
        new("1235", "Studio study")
    ];

    public static readonly User[] Users =
    [
        new("support@apollographql.com", 1337),
        new("studio@apollographql.com", 42)
    ];

    public static readonly ProductVariation[] ProductVariations =
    [
        new("OSS"),
        new("platform")
    ];

    public static readonly ProductDimension[] ProductDimensions =
    [
        new("small", 1.0, "kg"),
        new("large", 2.5, "kg")
    ];

    public static readonly ProductResearch[] ProductResearches =
    [
        new(CaseStudies[0], "Positive"),
        new(CaseStudies[1], "Inconclusive")
    ];

    public static readonly Product[] Products =
    [
        new(
            Id: "apollo-federation",
            Sku: "federation",
            Package: "@apollo/federation",
            Variation: ProductVariations[0],
            Dimensions: ProductDimensions[0],
            CreatedBy: Users[0],
            Notes: "Federation platform",
            Research: [ProductResearches[0]]
        ),
        new(
            Id: "apollo-studio",
            Sku: "studio",
            Package: "@apollo/studio",
            Variation: ProductVariations[1],
            Dimensions: ProductDimensions[1],
            CreatedBy: Users[1],
            Notes: "Studio platform",
            Research: [ProductResearches[1]]
        )
    ];

    public static readonly DeprecatedProduct[] DeprecatedProducts =
    [
        new("apollo-federation-v1", "@apollo/federation-v1", "Migrate to Federation v2", Users[0])
    ];

    public static readonly Inventory[] Inventories =
    [
        new("apollo-oss", DeprecatedProducts)
    ];
}