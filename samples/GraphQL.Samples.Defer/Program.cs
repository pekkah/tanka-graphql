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
    .AddSchema("UserProfile", schema =>
    {
        // Configure schema with @defer support
        schema.AddIncrementalDeliveryDirectives();

        // Define Query root
        schema.Add("Query", new FieldsWithResolvers
        {
            { "me: User!", GetCurrentUser },
            { "user(id: ID!): User", GetUserById }
        });

        // Define User type with fast and slow fields
        schema.Add("User", new FieldsWithResolvers
        {
            // Fast fields - basic info that loads quickly
            { "id: ID!", (User objectValue) => objectValue.Id },
            { "username: String!", (User objectValue) => objectValue.Username },
            { "displayName: String!", (User objectValue) => objectValue.DisplayName },
            { "avatarUrl: String", (User objectValue) => objectValue.AvatarUrl },
            
            // Slow fields - expensive data that can be deferred
            { "profile: UserProfile!", GetUserProfile },
            { "stats: UserStats!", CalculateUserStats },
            { "recentActivity: [Activity!]!", GetRecentActivity }
        });

        // UserProfile - detailed info that might require DB lookups
        schema.Add("UserProfile", new FieldsWithResolvers
        {
            { "bio: String", (UserProfile objectValue) => objectValue.Bio },
            { "location: String", (UserProfile objectValue) => objectValue.Location },
            { "website: String", (UserProfile objectValue) => objectValue.Website },
            { "joinedAt: String!", (UserProfile objectValue) => objectValue.JoinedAt.ToString("yyyy-MM-dd") },
            { "followers: Int!", (UserProfile objectValue) => objectValue.Followers },
            { "following: Int!", (UserProfile objectValue) => objectValue.Following }
        });

        // UserStats - expensive calculations
        schema.Add("UserStats", new FieldsWithResolvers
        {
            { "totalPosts: Int!", (UserStats objectValue) => objectValue.TotalPosts },
            { "totalLikes: Int!", (UserStats objectValue) => objectValue.TotalLikes },
            { "totalComments: Int!", (UserStats objectValue) => objectValue.TotalComments },
            { "engagementRate: Float!", (UserStats objectValue) => objectValue.EngagementRate },
            { "topTags: [String!]!", (UserStats objectValue) => objectValue.TopTags }
        });

        // Activity type for recent activity
        schema.Add("Activity", new FieldsWithResolvers
        {
            { "id: ID!", (Activity objectValue) => objectValue.Id },
            { "type: String!", (Activity objectValue) => objectValue.Type },
            { "description: String!", (Activity objectValue) => objectValue.Description },
            { "timestamp: String!", (Activity objectValue) => objectValue.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") }
        });
    });

var app = builder.Build();

app.UseWebSockets();
app.MapTankaGraphQL("/graphql", "UserProfile");
app.MapGraphiQL("/graphql/ui");

app.Run();

// Resolvers

static User GetCurrentUser()
{
    // Simulate fast data retrieval - basic user info
    return new User
    {
        Id = "user-123",
        Username = "johndoe",
        DisplayName = "John Doe",
        AvatarUrl = "https://example.com/avatars/johndoe.jpg"
    };
}

static User? GetUserById(string id)
{
    // Simulate user lookup by ID
    if (id == "user-123")
    {
        return GetCurrentUser();
    }
    return null;
}

static async Task<UserProfile> GetUserProfile(User objectValue)
{
    // Simulate expensive profile data fetch (e.g., from database)
    await Task.Delay(1500); // Simulate network/DB latency

    return new UserProfile
    {
        Bio = "Software developer passionate about GraphQL and distributed systems.",
        Location = "San Francisco, CA",
        Website = "https://johndoe.dev",
        JoinedAt = DateTime.UtcNow.AddYears(-3),
        Followers = 1234,
        Following = 567
    };
}

static async Task<UserStats> CalculateUserStats(User objectValue)
{
    // Simulate expensive statistics calculation
    await Task.Delay(2000); // Simulate complex aggregation query

    return new UserStats
    {
        TotalPosts = 456,
        TotalLikes = 12345,
        TotalComments = 2341,
        EngagementRate = 4.7f,
        TopTags = new[] { "graphql", "dotnet", "api", "microservices", "cloud" }
    };
}

static async Task<List<Activity>> GetRecentActivity(User objectValue)
{
    // Simulate fetching recent activity
    await Task.Delay(1000); // Simulate query time

    return new List<Activity>
    {
        new() { Id = "act-1", Type = "post", Description = "Published article about GraphQL @defer", Timestamp = DateTime.UtcNow.AddHours(-2) },
        new() { Id = "act-2", Type = "comment", Description = "Commented on 'Microservices Best Practices'", Timestamp = DateTime.UtcNow.AddHours(-5) },
        new() { Id = "act-3", Type = "like", Description = "Liked 'Introduction to Event Sourcing'", Timestamp = DateTime.UtcNow.AddDays(-1) },
        new() { Id = "act-4", Type = "post", Description = "Shared thoughts on system design", Timestamp = DateTime.UtcNow.AddDays(-2) },
        new() { Id = "act-5", Type = "follow", Description = "Started following @graphql", Timestamp = DateTime.UtcNow.AddDays(-3) }
    };
}

// Data models

record User
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
}

record UserProfile
{
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public string? Website { get; init; }
    public required DateTime JoinedAt { get; init; }
    public required int Followers { get; init; }
    public required int Following { get; init; }
}

record UserStats
{
    public required int TotalPosts { get; init; }
    public required int TotalLikes { get; init; }
    public required int TotalComments { get; init; }
    public required float EngagementRate { get; init; }
    public required string[] TopTags { get; init; }
}

record Activity
{
    public required string Id { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }
    public required DateTime Timestamp { get; init; }
}