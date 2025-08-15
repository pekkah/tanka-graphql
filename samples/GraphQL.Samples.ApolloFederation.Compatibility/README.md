# Tanka GraphQL Apollo Federation Compatibility Sample

This sample demonstrates Tanka GraphQL's compatibility with Apollo Federation v2.3 specification. It implements the standard Apollo Federation subgraph compatibility test schema for testing federation features.

## Features

- ✅ Apollo Federation v2.3 with @link directive support
- ✅ Complete subgraph compatibility schema implementation
- ✅ Entity resolution with multiple @key strategies
- ✅ Advanced Federation directives (@shareable, @inaccessible, @override, @tag, etc.)
- ✅ Reference resolvers for entity federation
- ✅ Docker support for compatibility testing

## Quick Start

### Local Development

```bash
# Navigate to the sample directory
cd samples/GraphQL.Samples.ApolloFederation.Compatibility

# Run the application
dotnet run

# The GraphQL endpoint will be available at:
# http://localhost:4001/
```

### Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# The GraphQL endpoint will be available at:
# http://localhost:4001/
```

## Testing Federation Compatibility

This sample is designed to work with the Apollo Federation subgraph compatibility test suite:

```bash
# Install the compatibility testing tool
npm install -g @apollo/federation-subgraph-compatibility

# Test the running subgraph
npx apollo-federation-subgraph-compatibility 4001
```

## Endpoints

- **GraphQL**: `http://localhost:4001/` - Main GraphQL endpoint
- **Schema**: `http://localhost:4001/schema` - View the raw GraphQL schema
- **Health**: `http://localhost:4001/health` - Health check endpoint

## Sample Queries

### Product Query
```graphql
query {
  product(id: "apollo-federation") {
    id
    sku
    package
    dimensions {
      size
      weight
    }
    createdBy {
      email
      totalProductsCreated
    }
  }
}
```

### Federation Service Query
```graphql
query {
  _service {
    sdl
  }
}
```

### Entity Resolution Query
```graphql
query {
  _entities(representations: [
    { __typename: "Product", id: "apollo-studio" }
  ]) {
    ... on Product {
      id
      sku
      notes
    }
  }
}
```

## Schema Features

This sample implements all required Federation v2.3 features:

- **Multiple Keys**: Products can be resolved by `id`, `sku package`, or `sku variation { id }`
- **Entity Types**: Product, DeprecatedProduct, ProductResearch, Inventory
- **Advanced Directives**: @shareable, @inaccessible, @tag, @interfaceObject
- **Cross-Subgraph Relations**: User type extension with @provides
- **Deprecation**: Deprecated queries and fields

## Federation Architecture

```
┌─────────────────────┐
│   Users Subgraph    │ ← Provided by test framework
│   (External)        │
└─────────────────────┘
           │
           │ Federation
           │
┌─────────────────────┐
│  Products Subgraph  │ ← This Tanka GraphQL implementation
│  (This Sample)      │
└─────────────────────┘
           │
           │ Federation  
           │
┌─────────────────────┐
│ Inventory Subgraph  │ ← Provided by test framework
│   (External)        │
└─────────────────────┘
```

## Implementation Notes

- Uses Tanka GraphQL's native Federation v2.3 support with @link directives
- Implements proper entity resolution with reference resolvers
- Supports all Federation directive features required for compatibility
- Follows Apollo Federation subgraph specification exactly
- Ready for production federation environments