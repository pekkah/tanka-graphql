# @defer and @stream Directives Usage Guide

This guide covers the practical usage of `@defer` and `@stream` directives in Tanka GraphQL for incremental delivery.

## Server Configuration

### Service Registration

**Full Incremental Delivery Support**
```csharp
services.AddDefaultTankaGraphQLServices();
services.AddIncrementalDeliveryDirectives(); // Adds both @defer and @stream
```

**Individual Directive Support**
```csharp
services.AddDefaultTankaGraphQLServices();
services.AddDeferDirective();   // Only @defer directive
services.AddStreamDirective();  // Only @stream directive
```

### Schema Configuration

**Executable Schema Builder**
```csharp
var schema = await new ExecutableSchemaBuilder()
    .AddIncrementalDeliveryDirectives() // Registers both directives
    .Add("Query", queryResolvers)
    .Build();
```

**Manual Registration**
```csharp
var schema = await new ExecutableSchemaBuilder()
    .AddDeferDirective()
    .AddStreamDirective()
    .Add("Query", queryResolvers)
    .Build();
```

## @defer Directive Usage

For a complete, working example of @defer usage, see: [Defer Tutorial Basic Usage](https://github.com/pekkah/tanka-graphql/blob/main/tutorials/GraphQL.Tutorials.Getting-Started/DeferStreamTutorials.cs)

### Syntax Variations

**Basic Defer**
```graphql
{
  user {
    id
    name
    ... @defer {
      profile {
        bio
        avatar
      }
    }
  }
}
```

**Labeled Defer**
```graphql
{
  user {
    id
    name
    ... @defer(label: "userProfile") {
      profile {
        bio
        avatar
      }
    }
  }
}
```

**Variable Labels**
```graphql
query GetUser($profileLabel: String!) {
  user {
    id
    name
    ... @defer(label: $profileLabel) {
      profile {
        bio
        avatar
      }
    }
  }
}
```

### Multiple Deferred Fragments

```graphql
{
  user {
    id
    name

    ... @defer(label: "profile") {
      profile {
        bio
        avatar
      }
    }

    ... @defer(label: "posts") {
      posts {
        title
        content
      }
    }

    ... @defer(label: "friends") {
      friends {
        id
        name
      }
    }
  }
}
```

### Nested Defer

```graphql
{
  user {
    id
    name

    ... @defer(label: "level1") {
      profile {
        bio

        ... @defer(label: "level2") {
          settings {
            theme
            notifications
          }
        }
      }
    }
  }
}
```

## @stream Directive Usage

For a complete, working example of @stream usage, see: [Stream Tutorial Basic Usage](https://github.com/pekkah/tanka-graphql/blob/main/tutorials/GraphQL.Tutorials.Getting-Started/DeferStreamTutorials.cs)

### Syntax Variations

**Basic Stream**
```graphql
{
  products @stream {
    id
    name
    price
  }
}
```

**With Initial Count**
```graphql
{
  products @stream(initialCount: 3) {
    id
    name
    price
    description
  }
}
```

**Stream All Items**
```graphql
{
  products @stream(initialCount: 0) {
    id
    name
    price
  }
}
```

**Labeled Stream**
```graphql
{
  products @stream(initialCount: 2, label: "productList") {
    id
    name
    price
  }
}
```

### Stream with Nested Data

```graphql
{
  categories @stream(initialCount: 1) {
    id
    name
    products {
      id
      name
      price
    }
  }
}
```

### Conditional Streaming

```graphql
query GetProducts($shouldStream: Boolean = false, $initialCount: Int = 5) {
  products @stream(initialCount: $initialCount) @skip(if: $shouldStream) {
    id
    name
    price
  }

  # Fallback for non-streaming
  allProducts: products @include(if: $shouldStream) {
    id
    name
    price
  }
}
```

## Combined Usage Patterns

### Defer + Stream Together

For a complete example combining both directives, see: [Combined Defer and Stream Tutorial](https://github.com/pekkah/tanka-graphql/blob/main/tutorials/GraphQL.Tutorials.Getting-Started/DeferStreamTutorials.cs)

```graphql
{
  user {
    id
    name

    ... @defer(label: "posts") {
      posts @stream(initialCount: 3) {
        id
        title
        content

        ... @defer(label: "postStats") {
          likes
          comments
          shares
        }
      }
    }
  }
}
```

### Complex Data Hierarchies

```graphql
{
  dashboard {
    # Critical data first
    user {
      id
      name
    }

    # Analytics deferred
    ... @defer(label: "analytics") {
      analytics {
        totalViews

        # Large datasets streamed
        topPages @stream(initialCount: 5) {
          url
          views

          # Expensive metrics deferred per item
          ... @defer(label: "pageMetrics") {
            bounceRate
            timeOnPage
            conversions
          }
        }
      }
    }

    # Recent activity streamed
    ... @defer(label: "activity") {
      recentActivity @stream(initialCount: 2) {
        id
        type
        timestamp
        description
      }
    }
  }
}
```

## Field Resolution Patterns

### Proper Resolver Implementation

**Collection Resolvers for @stream**
```csharp
// ✅ Correct: Return actual collection
.Add("products: [Product]", b => b.ResolveAs(productList))

// ❌ Incorrect: Returns function instead of collection
.Add("products: [Product]", b => b.ResolveAs(() => productList))
```

**Async Resolvers for @defer**
```csharp
.Add("User", new()
{
    { "id: ID!", b => b.ResolveAsPropertyOf<User>(u => u.Id) },
    { "name: String!", b => b.ResolveAsPropertyOf<User>(u => u.Name) },
    {
        "profile: Profile",
        async context =>
        {
            // Expensive operation suitable for deferring
            await Task.Delay(100);
            var profile = await profileService.GetProfileAsync(context.Parent<User>().Id);
            context.ResolvedValue = profile;
        }
    }
})
```

## Response Structure Examples

### @defer Response Flow

**Initial Response**
```json
{
  "data": {
    "user": {
      "id": "1",
      "name": "John Doe"
    }
  },
  "hasNext": true
}
```

**Incremental Response**
```json
{
  "incremental": [
    {
      "label": "userProfile",
      "path": ["user"],
      "data": {
        "profile": {
          "bio": "Software developer",
          "avatar": "https://example.com/avatar.jpg"
        }
      }
    }
  ],
  "hasNext": false
}
```

### @stream Response Flow

**Initial Response**
```json
{
  "data": {
    "products": [
      {"id": "1", "name": "Product 1", "price": 100},
      {"id": "2", "name": "Product 2", "price": 200}
    ]
  },
  "hasNext": true
}
```

**Incremental Responses**
```json
{
  "incremental": [
    {
      "path": ["products"],
      "items": [
        {"id": "3", "name": "Product 3", "price": 300}
      ]
    }
  ],
  "hasNext": true
}
```

```json
{
  "incremental": [
    {
      "path": ["products"],
      "items": [
        {"id": "4", "name": "Product 4", "price": 400},
        {"id": "5", "name": "Product 5", "price": 500}
      ]
    }
  ],
  "hasNext": false
}
```

## Error Handling

### Partial Errors in Deferred Data

```json
{
  "incremental": [
    {
      "label": "userProfile",
      "path": ["user"],
      "data": {
        "profile": null
      },
      "errors": [
        {
          "message": "Profile service unavailable",
          "path": ["user", "profile"]
        }
      ]
    }
  ],
  "hasNext": false
}
```

### Stream Item Errors

```json
{
  "incremental": [
    {
      "path": ["products"],
      "items": [null, null, {"id": "3", "name": "Product 3", "price": 300}],
      "errors": [
        {
          "message": "Product not found",
          "path": ["products", 0]
        },
        {
          "message": "Access denied",
          "path": ["products", 1]
        }
      ]
    }
  ],
  "hasNext": true
}
```

## Performance Optimization

### Batching Strategies
- Group related deferred fields under single labels
- Use appropriate `initialCount` values for streams
- Consider network round-trip costs vs. data size

### Memory Management
- Stream large datasets instead of loading everything
- Use defer for expensive computations
- Monitor server memory usage during streaming

### Caching Considerations
- Incremental responses complicate caching
- Consider caching strategies for initial vs. deferred data
- Use labels consistently for cache key generation

## Testing Strategies

### Unit Testing Resolvers

Complete unit tests can be found in:
- [StreamDirectiveExecutionTests.cs](https://github.com/pekkah/tanka-graphql/blob/main/tests/GraphQL.Tests/SelectionSets/StreamDirectiveExecutionTests.cs)

### Integration Testing

Complete integration tests can be found in:
- [DeferStreamIntegrationFacts.cs](https://github.com/pekkah/tanka-graphql/blob/main/tests/GraphQL.Tests/DeferStreamIntegrationFacts.cs)

### Manual Testing Commands

**Test @defer**
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Accept: multipart/mixed" \
  -d '{"query": "{ user { id name ... @defer(label: \"profile\") { profile { email } } } }"}'
```

**Test @stream**
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Accept: multipart/mixed" \
  -d '{"query": "{ products @stream(initialCount: 2) { id name } }"}'
```