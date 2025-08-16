using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Fields;
using Tanka.GraphQL.Language.Nodes.TypeSystem;
using Tanka.GraphQL.Request;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Extensions.ApolloFederation.Tests;

/// <summary>
/// Clean code examples for Apollo Federation documentation
/// These contain only the relevant code without test attributes
/// </summary>
public class FederationDocumentationCodeExamples
{
    public static async Task BasicFederationExample()
    {
        // 1. Define schema with @link directive for Federation v2.3
        var schema = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key", "@external", "@requires", "@provides"])

            type Product @key(fields: "id") {
                id: ID!
                name: String
                price: Float
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        // 2. Configure reference resolvers for entity resolution
        var referenceResolvers = new DictionaryReferenceResolversMap
        {
            ["Product"] = (context, type, representation) =>
            {
                var id = representation.GetValueOrDefault("id")?.ToString();
                var product = GetProductById(id); // Your data access logic
                return ValueTask.FromResult(new ResolveReferenceResult(type, product));
            }
        };

        // 3. Build executable schema with federation support
        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schema)
            .Add("Query", new FieldsWithResolvers
            {
                ["product(id: ID!): Product"] = b => b.Run(context =>
                {
                    var id = context.GetArgument<string>("id");
                    context.ResolvedValue = GetProductById(id);
                    return ValueTask.CompletedTask;
                })
            })
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(referenceResolvers));
            });
    }

    public static async Task MiddlewarePipelineIntegration()
    {
        var schemaSDL = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key", "@external"])

            type Product @key(fields: "id") {
                id: ID!
                name: String
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        var resolversBuilder = new ResolversBuilder();
        resolversBuilder.Resolver("Query", "product").Run(context =>
        {
            context.ResolvedValue = new { id = "1", name = "Test Product" };
            return ValueTask.CompletedTask;
        });

        var subgraphOptions = new SubgraphOptions(new DictionaryReferenceResolversMap());

        var schema = await new SchemaBuilder()
            .Add(schemaSDL)
            .Build(options =>
            {
                options.Resolvers = resolversBuilder.BuildResolvers();
                options.UseFederation(subgraphOptions);
            });

        // The federation middleware automatically:
        // - Processes @link directives and imports required types
        // - Adds _service and _entities fields to the Query type
        // - Configures entity resolution based on your reference resolvers
        // - Generates proper subgraph SDL for federation
    }

    public static async Task ReferenceResolverExample()
    {
        var schema = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key"])

            type Product @key(fields: "id") @key(fields: "sku") {
                id: ID!
                sku: String!
                name: String
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        // Reference resolvers handle entity resolution when the gateway requests entities by their key fields
        var referenceResolvers = new DictionaryReferenceResolversMap
        {
            ["Product"] = async (context, type, representation) =>
            {
                // Extract key fields from representation
                var id = representation.GetValueOrDefault("id")?.ToString();
                var sku = representation.GetValueOrDefault("sku")?.ToString();

                // Resolve entity based on key fields
                Product? product = null;
                if (id != null)
                {
                    product = await GetProductByIdAsync(id);
                }
                else if (sku != null)
                {
                    product = await GetProductBySkuAsync(sku);
                }

                return new ResolveReferenceResult(type, product);
            }
        };

        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(referenceResolvers));
            });
    }

    public static async Task FederationDirectivesExample()
    {
        // Tanka GraphQL supports all Apollo Federation v2.3 directives
        var schema = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key", "@external", "@requires", "@provides", 
                                       "@shareable", "@inaccessible", "@override", 
                                       "@tag", "@extends", "@composeDirective", "@interfaceObject"])

            type Product @key(fields: "id") @shareable {
                id: ID!
                name: String @inaccessible
                price: Float @tag(name: "public")
                weight: Float @external
                shippingCost: Float @requires(fields: "weight")
            }

            type Review @key(fields: "id") {
                id: ID!
                product: Product @provides(fields: "name")
                rating: Int
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(new DictionaryReferenceResolversMap()));
            });
    }

    public static async Task MigrationFromV1Example()
    {
        // Federation v2.3 - Use @link directive instead of manually defining federation types
        var schemaV2 = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                                import: ["@key", "@external"])

            type Product @key(fields: "id") {
                id: ID!
                name: String
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schemaV2)
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(new DictionaryReferenceResolversMap()));
            });

        // The middleware automatically handles the migration and adds the required fields
    }

    public static async Task SpecificImportsExample()
    {
        // Best practice: Only import the federation directives you actually use
        var schema = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key", "@shareable"])

            type Product @key(fields: "id") @shareable {
                id: ID!
                name: String
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(new DictionaryReferenceResolversMap()));
            });
    }

    public static async Task ErrorHandlingExample()
    {
        var schema = """
            extend schema @link(url: "https://specs.apollo.dev/federation/v2.3", 
                               import: ["@key"])

            type Product @key(fields: "id") {
                id: ID!
                name: String
            }

            type Query {
                product(id: ID!): Product
            }
            """;

        // Handle null entities - Reference resolvers should handle cases where entities don't exist
        var referenceResolvers = new DictionaryReferenceResolversMap
        {
            ["Product"] = (context, type, representation) =>
            {
                var id = representation.GetValueOrDefault("id")?.ToString();

                // Handle missing entities gracefully
                if (string.IsNullOrEmpty(id))
                {
                    return ValueTask.FromResult(new ResolveReferenceResult(type, null));
                }

                var product = GetProductById(id);
                if (product == null)
                {
                    // Return null for non-existent entities
                    return ValueTask.FromResult(new ResolveReferenceResult(type, null));
                }

                return ValueTask.FromResult(new ResolveReferenceResult(type, product));
            }
        };

        var executableSchema = await new ExecutableSchemaBuilder()
            .Add(schema)
            .Build(options =>
            {
                options.UseFederation(new SubgraphOptions(referenceResolvers));
            });
    }

    // Helper methods for examples
    private static Product? GetProductById(string? id)
    {
        if (id == "1")
            return new Product { Id = "1", Name = "Test Product", Price = 99.99f };
        return null;
    }

    private static Task<Product?> GetProductByIdAsync(string id)
    {
        return Task.FromResult(GetProductById(id));
    }

    private static Task<Product?> GetProductBySkuAsync(string sku)
    {
        if (sku == "SKU123")
            return Task.FromResult<Product?>(new Product { Id = "1", Sku = "SKU123", Name = "Test Product" });
        return Task.FromResult<Product?>(null);
    }

    private class Product
    {
        public string Id { get; set; } = "";
        public string? Sku { get; set; }
        public string Name { get; set; } = "";
        public float Price { get; set; }
    }
}