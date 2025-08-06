using Tanka.GraphQL;
using Tanka.GraphQL.Executable;
using Tanka.GraphQL.Server;
using Tanka.GraphQL.TypeSystem;

var builder = WebApplication.CreateBuilder(args);

// Add default GraphQL services and incremental delivery directives (@defer and @stream)
builder.Services.AddDefaultTankaGraphQLServices();
builder.Services.AddIncrementalDeliveryDirectives();

// Add Tanka GraphQL with incremental delivery support
builder.AddTankaGraphQL()
    .AddHttp()
    .AddWebSockets()
    .AddSchema("ProductCatalog", schema =>
    {
        // Configure schema with @stream support
        schema.AddIncrementalDeliveryDirectives();

        // Define Query root
        schema.Add("Query", new FieldsWithResolvers
        {
            { "products(category: String, limit: Int = 20): [Product!]!", GetProducts },
            { "product(id: ID!): Product", GetProductById },
            { "categories: [Category!]!", GetCategories },
            { "searchProducts(query: String!): [Product!]!", SearchProducts }
        });

        // Define Product type
        schema.Add("Product", new FieldsWithResolvers
        {
            { "id: ID!", (Product objectValue) => objectValue.Id },
            { "name: String!", (Product objectValue) => objectValue.Name },
            { "description: String!", (Product objectValue) => objectValue.Description },
            { "price: Float!", (Product objectValue) => objectValue.Price },
            { "category: Category!", (Product objectValue) => objectValue.Category },
            { "inStock: Boolean!", (Product objectValue) => objectValue.InStock },
            { "imageUrl: String", (Product objectValue) => objectValue.ImageUrl },
            
            // Related products - can be expensive to fetch
            { "relatedProducts: [Product!]!", GetRelatedProducts },
            
            // Reviews - often paginated and slow to fetch
            { "reviews: [Review!]!", GetProductReviews }
        });

        // Define Category type
        schema.Add("Category", new FieldsWithResolvers
        {
            { "id: ID!", (Category objectValue) => objectValue.Id },
            { "name: String!", (Category objectValue) => objectValue.Name },
            { "description: String", (Category objectValue) => objectValue.Description }
        });

        // Define Review type
        schema.Add("Review", new FieldsWithResolvers
        {
            { "id: ID!", (Review objectValue) => objectValue.Id },
            { "rating: Int!", (Review objectValue) => objectValue.Rating },
            { "comment: String!", (Review objectValue) => objectValue.Comment },
            { "author: String!", (Review objectValue) => objectValue.Author },
            { "createdAt: String!", (Review objectValue) => objectValue.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
        });
    });

var app = builder.Build();

app.UseWebSockets();
app.MapTankaGraphQL("/graphql", "ProductCatalog");
app.MapGraphiQL("/graphql/ui");

app.Run();

// Resolvers

static async Task<List<Product>> GetProducts(string? category, int limit)
{
    // Simulate database query with pagination
    await Task.Delay(200); // Simulate initial query setup time

    var products = GenerateProducts(100);

    // Filter by category if provided
    if (!string.IsNullOrEmpty(category))
    {
        products = products.Where(p => p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    return products.Take(limit).ToList();
}

static Product? GetProductById(string id)
{
    var products = GenerateProducts(100);
    return products.FirstOrDefault(p => p.Id == id);
}

static List<Category> GetCategories()
{
    return new List<Category>
    {
        new() { Id = "cat-1", Name = "Electronics", Description = "Electronic devices and accessories" },
        new() { Id = "cat-2", Name = "Books", Description = "Books and publications" },
        new() { Id = "cat-3", Name = "Clothing", Description = "Apparel and fashion items" },
        new() { Id = "cat-4", Name = "Home", Description = "Home and garden products" },
        new() { Id = "cat-5", Name = "Sports", Description = "Sports and outdoor equipment" }
    };
}

static async Task<List<Product>> SearchProducts(string query)
{
    // Simulate search processing time
    await Task.Delay(300); // Simulate search indexing time

    var products = GenerateProducts(100);
    var results = products
        .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
        .ToList();

    return results;
}

static async Task<List<Product>> GetRelatedProducts(Product product)
{
    // Simulate recommendation engine processing
    await Task.Delay(400); // Simulate recommendation engine delay

    var allProducts = GenerateProducts(100);
    var related = allProducts
        .Where(p => p.Category.Id == product.Category.Id && p.Id != product.Id)
        .Take(5)
        .ToList();

    return related;
}

static async Task<List<Review>> GetProductReviews(Product product)
{
    // Simulate fetching reviews from a review service
    await Task.Delay(500); // Simulate review service API delay

    var random = new Random(product.Id.GetHashCode());
    var reviewCount = random.Next(5, 20);
    var reviews = new List<Review>();

    for (int i = 0; i < reviewCount; i++)
    {
        reviews.Add(new Review
        {
            Id = $"review-{product.Id}-{i}",
            Rating = random.Next(1, 6),
            Comment = GetRandomReviewComment(random),
            Author = GetRandomAuthor(random),
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365))
        });
    }

    return reviews;
}

// Helper methods

static List<Product> GenerateProducts(int count)
{
    var categories = GetCategories();
    var products = new List<Product>();
    var random = new Random(42); // Seed for consistent data

    for (int i = 1; i <= count; i++)
    {
        var category = categories[random.Next(categories.Count)];
        products.Add(new Product
        {
            Id = $"prod-{i}",
            Name = $"{category.Name} Product {i}",
            Description = $"High quality {category.Name.ToLower()} product with amazing features",
            Price = Math.Round(random.NextDouble() * 1000 + 10, 2),
            Category = category,
            InStock = random.Next(100) > 10,
            ImageUrl = $"https://example.com/products/{i}.jpg"
        });
    }

    return products;
}

static string GetRandomReviewComment(Random random)
{
    var comments = new[]
    {
        "Excellent product! Highly recommended.",
        "Good value for money. Works as expected.",
        "Quality could be better, but it's okay for the price.",
        "Amazing! Exceeded my expectations.",
        "Not bad, but I've seen better.",
        "Perfect! Exactly what I was looking for.",
        "Disappointing. Didn't meet my expectations.",
        "Great product with fast delivery.",
        "Solid product. Would buy again.",
        "Average quality, nothing special."
    };

    return comments[random.Next(comments.Length)];
}

static string GetRandomAuthor(Random random)
{
    var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Robert", "Mary" };
    var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Davis", "Miller", "Wilson", "Moore", "Taylor" };

    return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
}

// Data models

record Product
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required double Price { get; init; }
    public required Category Category { get; init; }
    public required bool InStock { get; init; }
    public string? ImageUrl { get; init; }
}

record Category
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

record Review
{
    public required string Id { get; init; }
    public required int Rating { get; init; }
    public required string Comment { get; init; }
    public required string Author { get; init; }
    public required DateTime CreatedAt { get; init; }
}