using GraphQL.Samples.ApolloFederation.Compatibility;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Extensions.ApolloFederation;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.ValueResolution;

var builder = WebApplication.CreateBuilder(args);

// Read the GraphQL schema
var schemaContent = await File.ReadAllTextAsync("products.graphql");

// Create reference resolvers for Federation
var referenceResolvers = new DictionaryReferenceResolversMap
{
    ["Product"] = (context, type, representation) =>
    {
        var id = representation.GetValueOrDefault("id")?.ToString();
        var sku = representation.GetValueOrDefault("sku")?.ToString();
        var package = representation.GetValueOrDefault("package")?.ToString();

        Product? product = null;

        if (id != null)
        {
            product = Data.Products.FirstOrDefault(p => p.Id == id);
        }
        else if (sku != null && package != null)
        {
            product = Data.Products.FirstOrDefault(p => p.Sku == sku && p.Package == package);
        }

        return ValueTask.FromResult(new ResolveReferenceResult(type, product));
    },

    ["DeprecatedProduct"] = (context, type, representation) =>
    {
        var sku = representation.GetValueOrDefault("sku")?.ToString();
        var package = representation.GetValueOrDefault("package")?.ToString();

        var product = Data.DeprecatedProducts.FirstOrDefault(p => p.Sku == sku && p.Package == package);
        return ValueTask.FromResult(new ResolveReferenceResult(type, product));
    },

    ["ProductResearch"] = (context, type, representation) =>
    {
        var caseNumber = ((Dictionary<string, object>)representation["study"])["caseNumber"]?.ToString();
        var research = Data.ProductResearches.FirstOrDefault(r => r.Study.CaseNumber == caseNumber);
        return ValueTask.FromResult(new ResolveReferenceResult(type, research));
    },

    ["Inventory"] = (context, type, representation) =>
    {
        var id = representation.GetValueOrDefault("id")?.ToString();
        var inventory = Data.Inventories.FirstOrDefault(i => i.Id == id);
        return ValueTask.FromResult(new ResolveReferenceResult(type, inventory));
    }
};

// Create Federation options
var federationOptions = new SubgraphOptions(referenceResolvers);

// Add Tanka GraphQL services with HTTP transport
builder.AddTankaGraphQL()
    .AddHttp()
    .AddSchema("products", schema =>
    {
        // Add the GraphQL schema from file first
        schema.Add(schemaContent);

        // Add resolvers for Query fields using FieldsWithResolvers
        schema.Add("Query", new FieldsWithResolvers
        {
            {
                "product(id: ID!): Product", (string id) =>
                {
                    return Data.Products.FirstOrDefault(p => p.Id == id);
                }
            },
            {
                "deprecatedProduct(sku: String!, package: String!): DeprecatedProduct", (string sku, string package) =>
                {
                    return Data.DeprecatedProducts.FirstOrDefault(p => p.Sku == sku && p.Package == package);
                }
            }
        });

        // Add resolvers for Product type
        schema.Add("Product", new FieldsWithResolvers
        {
            { "id: ID!", (Product objectValue) => objectValue.Id },
            { "sku: String", (Product objectValue) => objectValue.Sku },
            { "package: String", (Product objectValue) => objectValue.Package },
            { "variation: ProductVariation", (Product objectValue) => objectValue.Variation },
            { "dimensions: ProductDimension", (Product objectValue) => objectValue.Dimensions },
            { "createdBy: User", (Product objectValue) => objectValue.CreatedBy },
            { "notes: String", (Product objectValue) => objectValue.Notes },
            { "research: [ProductResearch!]!", (Product objectValue) => objectValue.Research }
        });

        // Add resolvers for other types
        schema.Add("ProductVariation", new FieldsWithResolvers
        {
            { "id: ID!", (ProductVariation objectValue) => objectValue.Id }
        });

        schema.Add("ProductDimension", new FieldsWithResolvers
        {
            { "size: String", (ProductDimension objectValue) => objectValue.Size },
            { "weight: Float", (ProductDimension objectValue) => objectValue.Weight },
            { "unit: String", (ProductDimension objectValue) => objectValue.Unit }
        });

        schema.Add("ProductResearch", new FieldsWithResolvers
        {
            { "study: CaseStudy!", (ProductResearch objectValue) => objectValue.Study },
            { "outcome: String", (ProductResearch objectValue) => objectValue.Outcome }
        });

        schema.Add("CaseStudy", new FieldsWithResolvers
        {
            { "caseNumber: ID!", (CaseStudy objectValue) => objectValue.CaseNumber },
            { "description: String", (CaseStudy objectValue) => objectValue.Description }
        });

        schema.Add("DeprecatedProduct", new FieldsWithResolvers
        {
            { "sku: String!", (DeprecatedProduct objectValue) => objectValue.Sku },
            { "package: String!", (DeprecatedProduct objectValue) => objectValue.Package },
            { "reason: String", (DeprecatedProduct objectValue) => objectValue.Reason },
            { "createdBy: User", (DeprecatedProduct objectValue) => objectValue.CreatedBy }
        });

        schema.Add("Inventory", new FieldsWithResolvers
        {
            { "id: ID!", (Inventory objectValue) => objectValue.Id },
            { "deprecatedProducts: [DeprecatedProduct!]!", (Inventory objectValue) => objectValue.DeprecatedProducts }
        });

        schema.Add("User", new FieldsWithResolvers
        {
            { "email: ID!", (User objectValue) => objectValue.Email },
            { "totalProductsCreated: Int", (User objectValue) => objectValue.TotalProductsCreated }
        });
    })
    .AddSchemaOptions("products", options =>
    {
        options.PostConfigure(schema => schema.ConfigureBuild(build =>
        {
            build.UseFederation(federationOptions);
        }));
    });

var app = builder.Build();

// Map GraphQL endpoint at root path (Apollo Federation expects this)
app.MapTankaGraphQL("/", "products");

// Health check endpoint (required for Apollo Federation compatibility tests)
app.MapGet("/health", () => "OK");

Console.WriteLine("🚀 Tanka GraphQL Apollo Federation Compatibility Server");
Console.WriteLine("📍 GraphQL endpoint: /");
Console.WriteLine("❤️  Health endpoint: /health");

app.Run();