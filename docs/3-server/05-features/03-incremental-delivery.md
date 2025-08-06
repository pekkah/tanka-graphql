# Incremental Delivery (@defer and @stream)

Tanka GraphQL supports incremental delivery using the GraphQL `@defer` and `@stream` directives, allowing you to optimize the user experience by delivering data progressively instead of waiting for the entire response.

## Overview

Incremental delivery helps reduce initial response times and provides better user experience by:

- **Reducing initial load time**: Send critical data first, optional data later
- **Improving perceived performance**: Users see content sooner
- **Better resource utilization**: Spread expensive operations over time
- **Enhanced user experience**: Progressive content loading

The implementation follows the [GraphQL incremental delivery specification draft](https://github.com/graphql/graphql-spec/pull/742) and supports both HTTP multipart responses and standard JSON responses.

## @defer Directive

The `@defer` directive allows you to defer the execution of expensive or non-critical fields until after the initial response.

### Basic Usage

```graphql
query GetUserProfile {
  user {
    id
    name
    # Critical data is returned immediately
    
    ... @defer(label: "profile") {
      profile {
        bio
        avatar
        # Optional profile data is deferred
      }
    }
  }
}
```

### With Variable Labels

```graphql
query GetUserProfile($deferLabel: String!) {
  user {
    id
    name
    ... @defer(label: $deferLabel) {
      profile {
        bio
        avatar
      }
    }
  }
}
```

### Response Format

The server returns multiple responses:

1. **Initial response** with critical data and `"hasNext": true`
2. **Incremental responses** with deferred data and path information
3. **Final response** with `"hasNext": false`

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

```json
{
  "incremental": [
    {
      "label": "profile",
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

## @stream Directive

The `@stream` directive allows you to stream list items progressively instead of waiting for all items to be resolved.

### Basic Usage

```graphql
query GetProducts {
  products @stream(initialCount: 2) {
    id
    name
    price
  }
}
```

### Stream All Items

```graphql
query GetAllProducts {
  products @stream(initialCount: 0) {
    id
    name
    price
  }
}
```

### Response Format

The server streams list items progressively:

1. **Initial response** with the specified number of initial items
2. **Incremental responses** with individual items or batches
3. **Final response** when all items are delivered

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

## Best Practices

### When to Use @defer
- **Expensive computations**: Complex calculations that take time
- **Optional content**: Data that's not immediately visible (below the fold)
- **External dependencies**: Data from slow external APIs
- **Large nested objects**: Heavy data structures that aren't critical

### When to Use @stream  
- **Large collections**: Lists with many items
- **Real-time updates**: Items that arrive over time
- **Pagination alternative**: Progressive loading without explicit paging
- **Search results**: Show results as they're found

### Performance Considerations
- **Network overhead**: Multiple requests vs single large request
- **Server resources**: Streaming requires maintaining connection state
- **Client complexity**: Handling progressive updates requires more complex client code
- **Caching**: Incremental responses may complicate caching strategies

## Examples

### Working Examples

Complete working examples can be found in the test files and samples:

- **@defer Integration Test**: [DeferStreamIntegrationFacts.cs](https://github.com/pekkah/tanka-graphql/blob/main/tests/GraphQL.Tests/DeferStreamIntegrationFacts.cs)
- **@stream Unit Tests**: [StreamDirectiveExecutionTests.cs](https://github.com/pekkah/tanka-graphql/blob/main/tests/GraphQL.Tests/SelectionSets/StreamDirectiveExecutionTests.cs)
- **Defer Sample Project**: [GraphQL.Samples.Defer](https://github.com/pekkah/tanka-graphql/tree/main/samples/GraphQL.Samples.Defer)
- **Stream Sample Project**: [GraphQL.Samples.Stream](https://github.com/pekkah/tanka-graphql/tree/main/samples/GraphQL.Samples.Stream)