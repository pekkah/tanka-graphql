# GraphQL @stream Sample

This sample demonstrates the setup and schema structure for the `@stream` directive with Tanka GraphQL. While the directive recognition and infrastructure are implemented, full streaming execution is currently in development.

The `@stream` directive will be particularly useful for:

- Large lists that take time to fetch
- Paginated data from databases
- Search results that can be delivered as they're found
- Real-time data feeds

## What is @stream?

The `@stream` directive will allow you to stream items in a list field as they become available, rather than waiting for the entire list to be fetched. This will provide:

1. **Faster Initial Response**: Users see the first items immediately
2. **Progressive Loading**: Items appear as they're fetched
3. **Better UX**: Reduced perceived latency for large datasets
4. **Efficient Resource Usage**: Data can be fetched and sent in chunks

## Current Status

🚧 **Note**: The @stream directive infrastructure is implemented and the directive is recognized, but the full streaming execution is currently in development. The sample currently executes normally and returns complete lists, but demonstrates the schema structure and queries that will work with streaming once fully implemented.

## Running the Sample

```bash
cd samples/GraphQL.Samples.Stream
dotnet run
```

Then navigate to http://localhost:5241/graphql/ui to access GraphiQL.

## Example Queries

### Basic Product List with @stream

Stream products as they're fetched from the "database":

```graphql
query GetProductsWithStream {
  products(limit: 10) @stream(initialCount: 3) {
    id
    name
    price
    inStock
  }
}
```

This will:
1. Return the first 3 products immediately
2. Stream the remaining 7 products as they become available

### Streaming Search Results

Stream search results as they're found:

```graphql
query SearchWithStream {
  searchProducts(query: "Electronics") @stream(initialCount: 0) {
    id
    name
    description
    price
    category {
      name
    }
  }
}
```

### Nested Streaming - Product Reviews

Stream reviews for a specific product:

```graphql
query GetProductWithStreamedReviews {
  product(id: "prod-1") {
    id
    name
    price
    
    # Stream reviews as they're fetched
    reviews @stream(initialCount: 2, label: "productReviews") {
      id
      rating
      comment
      author
      createdAt
    }
  }
}
```

### Combining @stream with Field Selection

Stream products with their related products also streamed:

```graphql
query StreamProductsWithRelated {
  products(category: "Electronics", limit: 5) @stream(initialCount: 1, label: "mainProducts") {
    id
    name
    price
    
    # Each product's related items are also streamed
    relatedProducts @stream(initialCount: 0, label: "related") {
      id
      name
      price
    }
  }
}
```

## Response Format

When using `@stream`, the response is delivered as multipart/mixed with incremental payloads:

### Initial Response
```json
{
  "data": {
    "products": [
      { "id": "prod-1", "name": "Electronics Product 1", "price": 299.99 }
    ]
  },
  "hasNext": true
}
```

### Subsequent Chunks
```json
{
  "incremental": [
    {
      "items": [
        { "id": "prod-2", "name": "Electronics Product 2", "price": 499.99 }
      ],
      "path": ["products", 1],
      "label": "mainProducts"
    }
  ],
  "hasNext": true
}
```

### Final Chunk
```json
{
  "incremental": [
    {
      "items": [
        { "id": "prod-5", "name": "Electronics Product 5", "price": 899.99 }
      ],
      "path": ["products", 4]
    }
  ],
  "hasNext": false
}
```

## Implementation Details

The sample uses `IAsyncEnumerable<T>` to implement streaming:

```csharp
static async IAsyncEnumerable<Product> GetProducts(string? category, int limit)
{
    var products = GenerateProducts(100);
    
    foreach (var product in products.Take(limit))
    {
        await Task.Delay(100); // Simulate database latency
        yield return product;
    }
}
```

This allows Tanka GraphQL to:
1. Start sending the response immediately
2. Stream items as they're yielded
3. Handle backpressure naturally
4. Cancel enumeration if the client disconnects

## Performance Considerations

- Use `initialCount` to balance between initial response time and number of chunks
- Consider the overhead of each chunk when setting stream parameters
- Monitor the total time for streaming vs. non-streaming responses
- Use labels to identify different streams in the response

## Browser Support

The multipart/mixed responses work with:
- GraphiQL (as demonstrated)
- Apollo Client with multipart support
- Any client that supports the GraphQL multipart response specification